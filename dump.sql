BEGIN TRANSACTION;
CREATE TABLE "Categories" (
    "CategoryId" INTEGER NOT NULL CONSTRAINT "PK_Categories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);
INSERT INTO "Categories" VALUES(1,'Electronics-AAA');
INSERT INTO "Categories" VALUES(2,'Home-Goods');
INSERT INTO "Categories" VALUES(3,'blablablalblabla');
CREATE TABLE "Deliveries" (
    "DeliveryId" INTEGER NOT NULL CONSTRAINT "PK_Deliveries" PRIMARY KEY AUTOINCREMENT,
    "ProductId" INTEGER NOT NULL,
    "WarehouseId" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "DeliveryDate" TEXT NOT NULL,
    CONSTRAINT "FK_Deliveries_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("ProductId") ON DELETE CASCADE,
    CONSTRAINT "FK_Deliveries_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("WarehouseId") ON DELETE CASCADE
);
INSERT INTO "Deliveries" VALUES(1,1,1,60,'2026-10-28 00:00:00');
INSERT INTO "Deliveries" VALUES(2,2,1,12,'2026-10-22 00:00:00');
CREATE TABLE "Inventory" (
    "ProductId" INTEGER NOT NULL,
    "WarehouseId" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    CONSTRAINT "PK_Inventory" PRIMARY KEY ("ProductId", "WarehouseId"),
    CONSTRAINT "FK_Inventory_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("ProductId") ON DELETE CASCADE,
    CONSTRAINT "FK_Inventory_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("WarehouseId") ON DELETE CASCADE
);
INSERT INTO "Inventory" VALUES(1,1,57);
INSERT INTO "Inventory" VALUES(2,1,34);
INSERT INTO "Inventory" VALUES(3,2,54);
CREATE TABLE "Products" (
    "ProductId" INTEGER NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Price" decimal(10,2) NOT NULL,
    "CategoryId" INTEGER NOT NULL,
    CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("CategoryId") ON DELETE CASCADE
);
INSERT INTO "Products" VALUES(1,'Keyboard',15,1);
INSERT INTO "Products" VALUES(2,'Toilet Brush',5,2);
INSERT INTO "Products" VALUES(3,'qdene',10,2);
CREATE TABLE "User" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_User" PRIMARY KEY AUTOINCREMENT,
    "Username" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" TEXT NOT NULL,
    "RefreshToken" TEXT NULL,
    "RefreshTokenExpiryTime" TEXT NULL
);
INSERT INTO "User" VALUES(1,'username','AQAAAAIAAYagAAAAELngGZlsQNp4yEp8BeuQesKe9l8lGTSA+RDnPr2SKV2HRW+bEkO+lbTGmmQrvqQSeg==','User','0NrMDTQvWZ5YU2jxZk9OilmU3cbcJHIBD_1RNVEfo5Q','2026-11-04 07:34:13.8762228');
INSERT INTO "User" VALUES(2,'usera','AQAAAAIAAYagAAAAEP5ZLYytgn8HNrOQ7IbG17dxNHZMGiq93xFODO+ApeVm8Bd9GrNCGXiFqLz6mozubA==','User','F-_ggCYwc0qdQX89ImFYasbCuYzu-IJ2k5YCVbfbrD4','2026-11-04 07:34:55.2664989');
INSERT INTO "User" VALUES(3,'userb','AQAAAAIAAYagAAAAEEJ2MH61I7xZyA5PD8lY0PEBkcT/h2fbP+dEF2mboFr7G1uVtdVTJutSkrt/+3tCcA==','User','762qIeLoWz5AYEGeRS6U7Bl8xCm155XwmWMtJZuSryA','2026-11-04 07:36:38.7852953');
INSERT INTO "User" VALUES(4,'lecho','AQAAAAIAAYagAAAAEOa0iIq+XEHVx2vNJVN7sSfOzGFZ2tk+hP0t7Ds08Ji1h9QgT3+OS9snhFEwCErIVA==','User','kThW8eK3gIgNnichaEK5mNdvdo4g0SPwuUJttpAhW7Q','2026-11-04 08:02:05.9232402');
CREATE TABLE "Warehouses" (
    "WarehouseId" INTEGER NOT NULL CONSTRAINT "PK_Warehouses" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);
INSERT INTO "Warehouses" VALUES(1,'Warehouse-Sofia-1');
INSERT INTO "Warehouses" VALUES(2,'maine deutche');
CREATE TABLE "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);
INSERT INTO "__EFMigrationsHistory" VALUES('20261028073210_InitialCreate','9.0.0');
CREATE TABLE "__EFMigrationsLock" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK___EFMigrationsLock" PRIMARY KEY,
    "Timestamp" TEXT NOT NULL
);
CREATE INDEX "IX_Deliveries_ProductId" ON "Deliveries" ("ProductId");
CREATE INDEX "IX_Deliveries_WarehouseId" ON "Deliveries" ("WarehouseId");
CREATE INDEX "IX_Inventory_WarehouseId" ON "Inventory" ("WarehouseId");
CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");
DELETE FROM "sqlite_sequence";
INSERT INTO "sqlite_sequence" VALUES('User',4);
INSERT INTO "sqlite_sequence" VALUES('Categories',3);
INSERT INTO "sqlite_sequence" VALUES('Warehouses',2);
INSERT INTO "sqlite_sequence" VALUES('Products',3);
INSERT INTO "sqlite_sequence" VALUES('Deliveries',2);
COMMIT;