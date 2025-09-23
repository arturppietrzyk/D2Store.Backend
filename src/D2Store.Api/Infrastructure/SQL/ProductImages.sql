USE D2Store

CREATE TABLE dbo.ProductImages (
    ProductImageId UNIQUEIDENTIFIER NOT NULL, 
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Location NVARCHAR(255) NOT NULL,
    IsPrimary BIT NOT NULL,
    CONSTRAINT PK_ProductImages
    PRIMARY KEY CLUSTERED (ProductImageId ASC),
    CONSTRAINT FK_ProductImages_Products 
    FOREIGN KEY (ProductId) 
    REFERENCES dbo.Products(ProductId)
    ON DELETE CASCADE
);
