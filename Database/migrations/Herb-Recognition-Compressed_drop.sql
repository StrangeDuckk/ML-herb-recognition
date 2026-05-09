-- Created by Redgate Data Modeler (https://datamodeler.redgate-platform.com)
-- Last modification date: 2026-05-06 19:18:55.967

-- foreign keys
ALTER TABLE Plant
    DROP CONSTRAINT Entity_Occurance;

ALTER TABLE Plant
    DROP CONSTRAINT Entity_Root;

ALTER TABLE Plant
    DROP CONSTRAINT Entity_Sap;

ALTER TABLE Plant
    DROP CONSTRAINT Entity_Stalk;

ALTER TABLE Flower
    DROP CONSTRAINT Flower_DictColor;

ALTER TABLE Flower
    DROP CONSTRAINT Flower_DictShape;

ALTER TABLE Flower
    DROP CONSTRAINT Flowers_Flavours;

ALTER TABLE Fruit
    DROP CONSTRAINT Fruit_DictColor;

ALTER TABLE Fruit
    DROP CONSTRAINT Fruit_DictShape;

ALTER TABLE Fruit
    DROP CONSTRAINT Fruit_DictSurface;

ALTER TABLE Fruit
    DROP CONSTRAINT Fruit_Flavor;

ALTER TABLE Fruit
    DROP CONSTRAINT Fruit_Thickness;

ALTER TABLE Hat
    DROP CONSTRAINT Hat_DictColor;

ALTER TABLE Hat
    DROP CONSTRAINT Hat_DictShape;

ALTER TABLE Hat
    DROP CONSTRAINT Hat_DictSurface;

ALTER TABLE Hat
    DROP CONSTRAINT Hat_Thickness;

ALTER TABLE HealthProperty
    DROP CONSTRAINT HealthProperties_ActiveSubstances;

ALTER TABLE HealthProperty
    DROP CONSTRAINT HealthProperties_Diseases;

ALTER TABLE Leaf
    DROP CONSTRAINT Leaf_DictSurface;

ALTER TABLE Leaf
    DROP CONSTRAINT Leaf_LeafColor;

ALTER TABLE Leaf
    DROP CONSTRAINT Leaf_LeafShape;

ALTER TABLE Leaf
    DROP CONSTRAINT Leaf_Thickness;

ALTER TABLE Leaf
    DROP CONSTRAINT Leafs_Flavours;

ALTER TABLE Picture
    DROP CONSTRAINT Pictures_Plants;

ALTER TABLE Plant
    DROP CONSTRAINT Plants_Flower;

ALTER TABLE Plant
    DROP CONSTRAINT Plants_Fruit;

ALTER TABLE Plant
    DROP CONSTRAINT Plants_Hat;

ALTER TABLE Plant
    DROP CONSTRAINT Plants_Leaf;

ALTER TABLE Plant
    DROP CONSTRAINT Plants_Plants;

ALTER TABLE Plant
    DROP CONSTRAINT Plants_Poisonability;

ALTER TABLE Plant_Product
    DROP CONSTRAINT Plants_Properties_Plants;

ALTER TABLE Plant_Product
    DROP CONSTRAINT Plants_Properties_Products;

ALTER TABLE Plant
    DROP CONSTRAINT Plants_Type;

ALTER TABLE Product
    DROP CONSTRAINT Products_HealthProperties;

ALTER TABLE Product
    DROP CONSTRAINT Products_ProductTypes;

ALTER TABLE Root
    DROP CONSTRAINT Root_DictColor;

ALTER TABLE Root
    DROP CONSTRAINT Root_DictSurface;

ALTER TABLE Root
    DROP CONSTRAINT Root_Thickness;

ALTER TABLE Sap
    DROP CONSTRAINT Sap_DictColor;

ALTER TABLE Stalk
    DROP CONSTRAINT Stalk_DictColor;

ALTER TABLE Stalk
    DROP CONSTRAINT Stalk_DictShape;

ALTER TABLE Stalk
    DROP CONSTRAINT Stalk_DictSurface;

-- tables
DROP TABLE ActiveSubstance;

DROP TABLE Color;

DROP TABLE Disease;

DROP TABLE Flavour;

DROP TABLE Flower;

DROP TABLE Fruit;

DROP TABLE Hat;

DROP TABLE HealthProperty;

DROP TABLE Leaf;

DROP TABLE Occurance;

DROP TABLE Picture;

DROP TABLE Plant;

DROP TABLE PlantType;

DROP TABLE Plant_Product;

DROP TABLE Poisonability;

DROP TABLE Product;

DROP TABLE ProductType;

DROP TABLE Root;

DROP TABLE Sap;

DROP TABLE Shape;

DROP TABLE Stalk;

DROP TABLE Surface;

DROP TABLE Thickness;

drop table userinput;

-- End of file.

