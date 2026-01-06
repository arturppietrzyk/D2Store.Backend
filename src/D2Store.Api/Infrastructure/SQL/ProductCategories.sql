USE D2Store;
GO

CREATE TABLE ProductCategories
(
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_ProductCategories PRIMARY KEY (ProductId, CategoryId),
    CONSTRAINT FK_ProductCategories_Products FOREIGN KEY (ProductId) 
        REFERENCES Products (ProductId) ON DELETE CASCADE,
    CONSTRAINT FK_ProductCategories_Categories FOREIGN KEY (CategoryId) 
        REFERENCES Categories (CategoryId) ON DELETE CASCADE
);