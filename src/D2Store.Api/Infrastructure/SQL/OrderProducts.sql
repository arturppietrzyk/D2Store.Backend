USE D2Store

CREATE TABLE dbo.OrderProducts (
    OrderProductId UNIQUEIDENTIFIER NOT NULL,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Quantity INT NOT NULL,
    LastModified DATETIME NOT NULL,
    CONSTRAINT PK_OrderProducts PRIMARY KEY CLUSTERED (OrderProductId ASC),
    CONSTRAINT FK_OrderProducts_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderProducts_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
);