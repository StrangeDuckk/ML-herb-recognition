-- Created by Redgate Data Modeler (https://datamodeler.redgate-platform.com)
-- Last modification date: 2026-05-06 19:11:30.463

-- tables
-- Table: ActiveSubstance
CREATE TABLE ActiveSubstance (
    Id INT GENERATED ALWAYS AS IDENTITY,
    ActiveSubstance text  NOT NULL,
    CONSTRAINT ActiveSubstance_pk PRIMARY KEY (Id)
);

-- Table: Color
CREATE TABLE Color (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Color varchar(50)  NOT NULL,
    R int  NOT NULL,
    G int  NOT NULL,
    B int  NOT NULL,
    CONSTRAINT Color_pk PRIMARY KEY (Id)
);

-- Table: Disease
CREATE TABLE Disease (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Name varchar(300)  NOT NULL,
    Symptoms text  NOT NULL,
    CONSTRAINT Disease_pk PRIMARY KEY (Id)
);

-- Table: Flavour
CREATE TABLE Flavour (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Flavour varchar(50)  NOT NULL,
    CONSTRAINT Flavour_pk PRIMARY KEY (Id)
);

-- Table: Flower
CREATE TABLE Flower (
    Id INT GENERATED ALWAYS AS IDENTITY,
    SizeInCm decimal(5,2)  NOT NULL,
    ColorId int NOT NULL, --DODAWANE RECZNIE NOT NULL ZAMIAST GENERATED ALWAYS AS IDENTITY
    ShapeId int NOT NULL, --DODAWANE RECZNIE NOT NULL ZAMIAST GENERATED ALWAYS AS IDENTITY
    FlavourId int  NULL,
    ScentPower int  NOT NULL,
    CONSTRAINT Flower_pk PRIMARY KEY (Id)
);

-- Table: Fruit
CREATE TABLE Fruit (
    Id INT GENERATED ALWAYS AS IDENTITY,
    FlavourId int  NULL,
    ShapeId int NOT NULL, --DODAWANE RECZNIE NOT NULL ZAMIAST GENERATED ALWAYS AS IDENTITY
    ColorId INT NOT NULL, --DODAWANE RECZNIE NOT NULL ZAMIAST GENERATED ALWAYS AS IDENTITY
    SurfaceId INT NOT NULL, --DODAWANE RECZNIE NOT NULL ZAMIAST GENERATED ALWAYS AS IDENTITY
    ThicknessId INT NOT NULL, --DODAWANE RECZNIE NOT NULL ZAMIAST GENERATED ALWAYS AS IDENTITY
    CONSTRAINT Fruit_pk PRIMARY KEY (Id)
);

-- Table: Hat
CREATE TABLE Hat (
    Id INT GENERATED ALWAYS AS IDENTITY,
    ColorId INT NOT NULL,
    ShapeId INT NOT NULL,
    ThicknessId INT NOT NULL,
    SurfaceId INT NOT NULL,
    HasSpots boolean  NOT NULL,
    HasGills boolean  NOT NULL,
    CONSTRAINT Hat_pk PRIMARY KEY (Id)
);

-- Table: HealthProperty
CREATE TABLE HealthProperty (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Property varchar(300)  NOT NULL,
    ActiveSubstanceId INT NOT NULL,
    DiseaseId INT NOT NULL,
    CONSTRAINT HealthProperty_pk PRIMARY KEY (Id)
);

-- Table: Leaf
CREATE TABLE Leaf (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Stripes boolean  NOT NULL,
    Spots boolean  NOT NULL,
    Holes boolean  NOT NULL,
    LeafShapeId INT NOT NULL,
    LeafColorId INT NOT NULL,
    SurfaceId INT NOT NULL,
    LeafLength decimal(3,0)  NOT NULL,
    ThicknessId INT NOT NULL,
    FlavourId int  NULL,
    CONSTRAINT Leaf_pk PRIMARY KEY (Id)
);

-- Table: Occurance
CREATE TABLE Occurance (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Occurance varchar(150)  NOT NULL,
    CONSTRAINT Occurance_pk PRIMARY KEY (Id)
);

-- Table: Picture
CREATE TABLE Picture (
    Id INT GENERATED ALWAYS AS IDENTITY,
    PlantId INT NOT NULL,
    PictureLink text  NOT NULL,
    CONSTRAINT Picture_pk PRIMARY KEY (Id)
);

-- Table: Plant
CREATE TABLE Plant (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Name varchar(50)  NOT NULL,
    PolishName varchar(50)  NOT NULL,
    LatinName varchar(50)  NULL,
    Subriquet varchar(50)  NULL,
    PlantTypeId INT NOT NULL,
    SapId int  NULL,
    RootId int  NULL,
    StalkId INT NOT NULL,
    OccuranceId INT NOT NULL,
    HatId int  NULL,
    LeafId int  NULL,
    FlowerId int  NULL,
    FruitId int  NULL,
    SimilarPlantsId int  NULL,
    Poisonabilityid int  NULL,
    CONSTRAINT Plant_pk PRIMARY KEY (Id)
);

-- Table: PlantType
CREATE TABLE PlantType (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Name varchar(50)  NOT NULL,
    CONSTRAINT PlantType_pk PRIMARY KEY (Id)
);

-- Table: Plant_Product
CREATE TABLE Plant_Product (
    Id INT GENERATED ALWAYS AS IDENTITY,
    PlantsId INT NOT NULL,
    ProductsId INT NOT NULL,
    CONSTRAINT Plant_Product_pk PRIMARY KEY (Id)
);

-- Table: Poisonability
CREATE TABLE Poisonability (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Description varchar(300)  NOT NULL,
    CONSTRAINT Poisonability_pk PRIMARY KEY (id)
);

-- Table: Product
CREATE TABLE Product (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Name varchar(300)  NOT NULL,
    Recipe text  NOT NULL,
    HealthPropertyId INT NOT NULL,
    Contraindication varchar(100)  NULL,
    ProductTypeId INT NOT NULL,
    CONSTRAINT Product_pk PRIMARY KEY (Id)
);

-- Table: ProductType
CREATE TABLE ProductType (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Name varchar(50)  NOT NULL,
    CONSTRAINT ProductType_pk PRIMARY KEY (Id)
);

-- Table: Root
CREATE TABLE Root (
    Id INT GENERATED ALWAYS AS IDENTITY,
    ColorId INT NOT NULL,
    SurfaceId INT NOT NULL,
    ThicknessId INT NOT NULL,
    CONSTRAINT Root_pk PRIMARY KEY (Id)
);

-- Table: Sap
CREATE TABLE Sap (
    Id INT GENERATED ALWAYS AS IDENTITY,
    ColorId INT NOT NULL,
    LeavesStains boolean  NOT NULL,
    Sticky boolean  NOT NULL,
    CONSTRAINT Sap_pk PRIMARY KEY (Id)
);

-- Table: Shape
CREATE TABLE Shape (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Shape varchar(50)  NOT NULL,
    CONSTRAINT Shape_pk PRIMARY KEY (Id)
);

-- Table: Stalk
CREATE TABLE Stalk (
    Id INT GENERATED ALWAYS AS IDENTITY,
    ShapeId INT NOT NULL,
    ColorId INT NOT NULL,
    SurfaceId INT NOT NULL,
    CONSTRAINT Stalk_pk PRIMARY KEY (Id)
);

-- Table: Surface
CREATE TABLE Surface (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Surface varchar(50)  NOT NULL,
    CONSTRAINT Surface_pk PRIMARY KEY (Id)
);

-- Table: Thicknesse
CREATE TABLE Thickness (
    Id INT GENERATED ALWAYS AS IDENTITY,
    Thickness varchar(50)  NOT NULL,
    CONSTRAINT Thicknesse_pk PRIMARY KEY (Id)
);

-- foreign keys
-- Reference: Entity_Occurance (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Entity_Occurance
    FOREIGN KEY (OccuranceId)
    REFERENCES Occurance (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Entity_Root (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Entity_Root
    FOREIGN KEY (RootId)
    REFERENCES Root (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Entity_Sap (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Entity_Sap
    FOREIGN KEY (SapId)
    REFERENCES Sap (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Entity_Stalk (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Entity_Stalk
    FOREIGN KEY (StalkId)
    REFERENCES Stalk (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Flower_DictColor (table: Flower)
ALTER TABLE Flower ADD CONSTRAINT Flower_DictColor
    FOREIGN KEY (ColorId)
    REFERENCES Color (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Flower_DictShape (table: Flower)
ALTER TABLE Flower ADD CONSTRAINT Flower_DictShape
    FOREIGN KEY (ShapeId)
    REFERENCES Shape (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Flowers_Flavours (table: Flower)
ALTER TABLE Flower ADD CONSTRAINT Flowers_Flavours
    FOREIGN KEY (FlavourId)
    REFERENCES Flavour (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Fruit_DictColor (table: Fruit)
ALTER TABLE Fruit ADD CONSTRAINT Fruit_DictColor
    FOREIGN KEY (ColorId)
    REFERENCES Color (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Fruit_DictShape (table: Fruit)
ALTER TABLE Fruit ADD CONSTRAINT Fruit_DictShape
    FOREIGN KEY (ShapeId)
    REFERENCES Shape (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Fruit_DictSurface (table: Fruit)
ALTER TABLE Fruit ADD CONSTRAINT Fruit_DictSurface
    FOREIGN KEY (SurfaceId)
    REFERENCES Surface (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Fruit_Flavor (table: Fruit)
ALTER TABLE Fruit ADD CONSTRAINT Fruit_Flavor
    FOREIGN KEY (FlavourId)
    REFERENCES Flavour (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Fruit_Thickness (table: Fruit)
ALTER TABLE Fruit ADD CONSTRAINT Fruit_Thickness
    FOREIGN KEY (ThicknessId)
    REFERENCES Thickness (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Hat_DictColor (table: Hat)
ALTER TABLE Hat ADD CONSTRAINT Hat_DictColor
    FOREIGN KEY (ColorId)
    REFERENCES Color (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Hat_DictShape (table: Hat)
ALTER TABLE Hat ADD CONSTRAINT Hat_DictShape
    FOREIGN KEY (ShapeId)
    REFERENCES Shape (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Hat_DictSurface (table: Hat)
ALTER TABLE Hat ADD CONSTRAINT Hat_DictSurface
    FOREIGN KEY (SurfaceId)
    REFERENCES Surface (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Hat_Thickness (table: Hat)
ALTER TABLE Hat ADD CONSTRAINT Hat_Thickness
    FOREIGN KEY (ThicknessId)
    REFERENCES Thickness (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: HealthProperties_ActiveSubstances (table: HealthProperty)
ALTER TABLE HealthProperty ADD CONSTRAINT HealthProperties_ActiveSubstances
    FOREIGN KEY (ActiveSubstanceId)
    REFERENCES ActiveSubstance (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: HealthProperties_Diseases (table: HealthProperty)
ALTER TABLE HealthProperty ADD CONSTRAINT HealthProperties_Diseases
    FOREIGN KEY (DiseaseId)
    REFERENCES Disease (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Leaf_DictSurface (table: Leaf)
ALTER TABLE Leaf ADD CONSTRAINT Leaf_DictSurface
    FOREIGN KEY (SurfaceId)
    REFERENCES Surface (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Leaf_LeafColor (table: Leaf)
ALTER TABLE Leaf ADD CONSTRAINT Leaf_LeafColor
    FOREIGN KEY (LeafColorId)
    REFERENCES Color (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Leaf_LeafShape (table: Leaf)
ALTER TABLE Leaf ADD CONSTRAINT Leaf_LeafShape
    FOREIGN KEY (LeafShapeId)
    REFERENCES Shape (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Leaf_Thickness (table: Leaf)
ALTER TABLE Leaf ADD CONSTRAINT Leaf_Thickness
    FOREIGN KEY (ThicknessId)
    REFERENCES Thickness (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Leafs_Flavours (table: Leaf)
ALTER TABLE Leaf ADD CONSTRAINT Leafs_Flavours
    FOREIGN KEY (FlavourId)
    REFERENCES Flavour (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Pictures_Plants (table: Picture)
ALTER TABLE Picture ADD CONSTRAINT Pictures_Plants
    FOREIGN KEY (PlantId)
    REFERENCES Plant (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Plants_Flower (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Plants_Flower
    FOREIGN KEY (FlowerId)
    REFERENCES Flower (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Plants_Fruit (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Plants_Fruit
    FOREIGN KEY (FruitId)
    REFERENCES Fruit (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Plants_Hat (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Plants_Hat
    FOREIGN KEY (HatId)
    REFERENCES Hat (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Plants_Leaf (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Plants_Leaf
    FOREIGN KEY (LeafId)
    REFERENCES Leaf (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Plants_Plants (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Plants_Plants
    FOREIGN KEY (SimilarPlantsId)
    REFERENCES Plant (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Plants_Poisonability (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Plants_Poisonability
    FOREIGN KEY (PoisonabilityId)
    REFERENCES Poisonability (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Plants_Properties_Plants (table: Plant_Product)
ALTER TABLE Plant_Product ADD CONSTRAINT Plants_Properties_Plants
    FOREIGN KEY (PlantsId)
    REFERENCES Plant (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Plants_Properties_Products (table: Plant_Product)
ALTER TABLE Plant_Product ADD CONSTRAINT Plants_Properties_Products
    FOREIGN KEY (ProductsId)
    REFERENCES Product (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Plants_Type (table: Plant)
ALTER TABLE Plant ADD CONSTRAINT Plants_Type
    FOREIGN KEY (PlantTypeId)
    REFERENCES PlantType (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Products_HealthProperties (table: Product)
ALTER TABLE Product ADD CONSTRAINT Products_HealthProperties
    FOREIGN KEY (HealthPropertyId)
    REFERENCES HealthProperty (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Products_ProductTypes (table: Product)
ALTER TABLE Product ADD CONSTRAINT Products_ProductTypes
    FOREIGN KEY (ProductTypeId)
    REFERENCES ProductType (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Root_DictColor (table: Root)
ALTER TABLE Root ADD CONSTRAINT Root_DictColor
    FOREIGN KEY (ColorId)
    REFERENCES Color (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Root_DictSurface (table: Root)
ALTER TABLE Root ADD CONSTRAINT Root_DictSurface
    FOREIGN KEY (SurfaceId)
    REFERENCES Surface (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Root_Thickness (table: Root)
ALTER TABLE Root ADD CONSTRAINT Root_Thickness
    FOREIGN KEY (ThicknessId)
    REFERENCES Thickness (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Sap_DictColor (table: Sap)
ALTER TABLE Sap ADD CONSTRAINT Sap_DictColor
    FOREIGN KEY (ColorId)
    REFERENCES Color (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Stalk_DictColor (table: Stalk)
ALTER TABLE Stalk ADD CONSTRAINT Stalk_DictColor
    FOREIGN KEY (ColorId)
    REFERENCES Color (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Stalk_DictShape (table: Stalk)
ALTER TABLE Stalk ADD CONSTRAINT Stalk_DictShape
    FOREIGN KEY (ShapeId)
    REFERENCES Shape (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- Reference: Stalk_DictSurface (table: Stalk)
ALTER TABLE Stalk ADD CONSTRAINT Stalk_DictSurface
    FOREIGN KEY (SurfaceId)
    REFERENCES Surface (Id)  
    NOT DEFERRABLE 
    INITIALLY IMMEDIATE
;

-- End of file.

