USE D2Store

CREATE TABLE dbo.BasketProducts (
    BasketProductId UNIQUEIDENTIFIER NOT NULL,
    BasketId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Quantity INT NOT NULL,
    LastModified DATETIME NOT NULL,
    CONSTRAINT PK_BasketProducts PRIMARY KEY CLUSTERED (BasketProductId ASC),
    CONSTRAINT FK_BasketProducts_Baskets FOREIGN KEY (BasketId) REFERENCES dbo.Baskets(BasketId) ON DELETE CASCADE,
    CONSTRAINT FK_BasketProducts_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
);