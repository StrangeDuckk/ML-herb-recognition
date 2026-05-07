-- dodawanie danych
TRUNCATE TABLE activesubstance RESTART IDENTITY CASCADE;
INSERT INTO activesubstance (activesubstance) VALUES 
('Flavanoids'),
('Phytosterols'),
('Inulin'),
('Alkaloids'),
('Tannins'),
('Saponins'),
('Essential oils'),
('Glycosides'),
('Phenolic acids'),
('Terpenes'),
('Carotenoids'),
('Coumarins'),
('Anthocyanins'),
('Lignans'),
('Resins'),
('Mucilage'),
('Organic acids'),
('Vitamins'),
('Mineral compounds'),
('Bitters');

TRUNCATE TABLE color RESTART IDENTITY CASCADE;
INSERT INTO color (Color, R, G, B) VALUES
('White', 255, 255, 255),
('Cream', 245, 245, 220),
('Yellow', 255, 255, 0),
('Golden', 255, 215, 0),
('Orange', 255, 165, 0),
('Red', 255, 0, 0),
('Burgundy', 128, 0, 32),
('Dark Red', 139, 0, 0),
('Brown', 139, 69, 19),
('Dark Brown', 101, 67, 33),
('Beige', 210, 180, 140),
('Pink', 255, 192, 203),
('Rose Pink', 255, 102, 204),
('Purple', 128, 0, 128),
('Violet', 148, 0, 211),
('Blue', 0, 0, 255),
('Dark Blue', 0, 0, 139),
('Light Blue', 173, 216, 230),
('Turquoise', 64, 224, 208),
('Light Green', 144, 238, 144),
('Green', 0, 204, 0),
('Dark Green', 0, 128, 0),
('Olive Green', 107, 142, 35),
('Gray', 128, 128, 128),
('Dark Gray', 64, 64, 64),
('Black', 0, 0, 0),
('Transparent', 255, 255, 255);

TRUNCATE TABLE disease RESTART IDENTITY CASCADE;
INSERT INTO disease (Name, Symptoms) VALUES
('Flu', 'fever, chills, muscle pain, fatigue'),
('Cold', 'runny nose, cough, sore throat'),
('Psoriasis', 'red scaly skin, itching'),
('Acne', 'pimples, oily skin, inflammation'),
('Fungal infection', 'itching, redness, skin peeling'),
('Urinary tract infection', 'painful urination, frequent urination'),
('Indigestion', 'bloating, stomach pain, nausea'),
('Insomnia', 'difficulty falling asleep, fatigue'),
('Anxiety', 'restlessness, nervousness'),
('Headache', 'head pain, pressure'),
('Constipation', 'infrequent bowel movements'),
('Bronchitis', 'cough, mucus, chest discomfort'),
('Parasitic infection', 'abdominal pain, diarrhea, worms'),
('Wounds', 'skin damage, bleeding'),
('Burns', 'redness, pain, skin damage');

TRUNCATE TABLE flavour RESTART IDENTITY CASCADE;
INSERT INTO flavour (Flavour) VALUES
('Sweet'),
('Sour'),
('Bitter'),
('Spicy'),
('Hot'),
('Mild'),
('Suffocating'),
('Nice'),
('Fresh'),
('Herbal'),
('Earthy'),
('Woody'),
('Citrus'),
('Fruity'),
('Minty'),
('Floral'),
('Resinous'),
('Sharp'),
('Aromatic');

TRUNCATE TABLE shape RESTART IDENTITY CASCADE;
INSERT INTO shape (Shape) VALUES
('Round'),
('Oval'),
('Tearshape'),
('Rectangular'),
('Hexagonal'),
('Triangular'),
('Heart-shaped'),
('Linear'),
('Irregular');

TRUNCATE TABLE flower RESTART IDENTITY CASCADE;
INSERT INTO flower (SizeInCm, ColorId, ShapeId, FlavourId, ScentPower) VALUES
(3, 23, 8, NULL, 1),
(3, 4, 8, NULL, 1),
(4, 3, 1, 3, 1),
(30, 5, 8, NULL, 1),
(0.02, 20, 9, NULL, 1),
(0.03, 12, 8, 15, 3),
(2, 1, 1, 10, 2);

TRUNCATE TABLE surface RESTART IDENTITY CASCADE;
INSERT INTO surface (Surface) VALUES
('Smooth'),
('Rough'),
('Spikes'),
('Wet'),
('Dry'),
('Velvety'),
('Waxy'),
('Wrinkled'),
('Glossy'),
('Matte');

TRUNCATE TABLE thickness RESTART IDENTITY CASCADE;
INSERT INTO thickness (Thickness) VALUES
('Thin'),
('Limp'),
('Flexible'),
('Fleshy'),
('Dense'),
('Hard'),
('Woody');

TRUNCATE TABLE fruit RESTART IDENTITY CASCADE;
INSERT INTO fruit (FlavourId, ShapeId, ColorId, SurfaceId, ThicknessId) VALUES
(NULL, 2, 9, 5, 1),
(NULL, 3, 10, 2, 7),
(NULL, 9, 9, 5, 1),
(NULL, 8, 9, 5, 1),
(NULL, 2, 20, 5, 1);

TRUNCATE TABLE hat RESTART IDENTITY CASCADE;
INSERT INTO hat (ColorId, ShapeId, ThicknessId, SurfaceId, HasSpots, HasGills) VALUES
(5, 9, 4, 1, false, true),
(6, 1, 4, 6, true, true),
(4, 1, 5, 1, false, true);

TRUNCATE TABLE healthproperty RESTART IDENTITY CASCADE;
INSERT INTO healthproperty (Property, ActiveSubstanceId, DiseaseId) VALUES
('Anti-inflammatory', 1, 3),
('Antibacterial', 7, 4),
('Antiviral', 10, 1),
('Antifungal', 6, 5),
('Antioxidant', 1, 10),
('Diuretic', 9, 6),
('Digestive support', 20, 7),
('Immune boosting', 18, 2),
('Calming', 8, 9),
('Pain relieving', 4, 10),
('Detoxifying', 2, 6),
('Expectorant', 7, 12),
('Antiparasitic', 4, 13),
('Skin healing', 5, 14),
('Laxative', 16, 11);

TRUNCATE TABLE leaf RESTART IDENTITY CASCADE;
INSERT INTO leaf (Stripes, Spots, Holes, LeafShapeId, LeafColorId, SurfaceId, LeafLength, ThicknessId, FlavourId) VALUES
(false, false, false, 3, 21, 1, 5, 1, 3),
(false, false, false, 8, 22, 7, 5, 3, 17),
(false, false, false, 9, 21, 1, 10, 1, 3),
(false, true,  false, 8, 22, 7, 30, 4, 9),
(false, false, false, 3, 21, 3, 5, 1, 10),
(false, false, false, 2, 21, 6, 5, 2, 15),
(false, false, false, 9, 20, 10, 6, 2, 10);

TRUNCATE TABLE occurance RESTART IDENTITY CASCADE;
INSERT INTO occurance (Occurance) VALUES
('In full sun'),
('in partial shade'),
('in shade'),
('in moist soil'),
('in dry soil'),
('in wetlands'),
('in water'),
('in forests'),
('in meadows'),
('in mountains'),
('in fields'),
('in sandy soil'),
('in rocky areas'),
('in gardens'),
('in pots inside');

TRUNCATE TABLE root RESTART IDENTITY CASCADE;
INSERT INTO root (ColorId, SurfaceId, ThicknessId) VALUES
(11, 2, 5),
(10, 2, 7),
(11, 1, 5),
(9, 1, 5),
(11, 2, 3),
(11, 1, 2);

TRUNCATE TABLE sap RESTART IDENTITY CASCADE;
INSERT INTO sap (ColorId, LeavesStains, Sticky) VALUES
(27, false, false),
(4, true, true),
(1, true, false),
(27, false, true);

TRUNCATE TABLE STALK RESTART IDENTITY CASCADE;
INSERT INTO stalk (ShapeId, ColorId, SurfaceId) VALUES
(1, 1, 1),
(1, 8, 2),
(1, 21, 7),
(1, 3, 10),
(1, 1, 6),
(1, 22, 3),
(4, 22, 6),
(1, 21, 1);

TRUNCATE TABLE POISONABILITY RESTART IDENTITY CASCADE;
INSERT INTO poisonability (Description) VALUES
('May cause mild irritation or allergic reaction'),
('Can cause nausea, vomiting or skin irritation'),
('Toxic - causes serious symptoms like dizziness, strong vomiting, diarrhea'),
('Highly toxic - can cause organ damage, breathing problems'),
('Extremely toxic - can cause paralysis or death');

TRUNCATE TABLE PLANTTYPE RESTART IDENTITY CASCADE;
INSERT INTO planttype (Name) VALUES
('Tree'),
('Herb'),
('Fungus');

TRUNCATE TABLE plant RESTART IDENTITY CASCADE;
INSERT INTO plant (
    Name,
    PolishName,
    LatinName,
    Subriquet,
    PlantTypeId,
    SapId,
    RootId,
    StalkId,
    OccuranceId,
    HatId,
    LeafId,
    FlowerId,
    FruitId,
    SimilarPlantsId,
    PoisonabilityId
) VALUES
('Birch', 'Brzoza', 'Betula pendula', NULL, 1, 1, 1, 1, 8, NULL, 1, 1, 1, NULL, 1),
('Pine', 'Sosna', 'Pinus sylvestris', NULL, 1, 2, 2, 2, 8, NULL, 2, 2, 2, NULL, 1),
('Dandelion', 'Mniszek pospolity', 'Taraxacum officinale', 'Mniszek lekarski', 2, 3, 3, 3, 9, NULL, 3, 3, 3, NULL, 1),
('Aloe', 'Aloes', 'Aloe vera', NULL, 2, 4, 4, 1, 15, NULL, 4, 4, 4, NULL, 1),
('Chanterelle', 'Pieprznik jadalny', 'Cantharellus cibarius', 'Kurka', 3, NULL, NULL, 4, 8, 1, NULL, NULL, NULL, NULL, 1),
('Fly agaric', 'Muchomor czerwony', 'Amanita muscaria', NULL, 3, NULL, NULL, 5, 8, 2, NULL, NULL, NULL, NULL, 4),
('Champignon', 'Pieczarka dwuzarodnikowa', 'Agaricus bisporus', 'Pieczarka', 3, NULL, NULL, 2, 9, 3, NULL, NULL, NULL, NULL, 1),
('Nettle', 'Pokrzywa zwyczajna', 'Urtica dioica', 'Pokrzywa', 2, NULL, 5, 6, 4, NULL, 5, 5, 5, NULL, 2),
('Mint', 'Mięta pieprzowa', 'Mentha piperita', NULL, 2, NULL, 6, 7, 9, NULL, 6, 6, 1, NULL, 1),
('Chamomile', 'Rumianek', 'Matricaria', NULL, 2, NULL, 6, 8, 9, NULL, 7, 7, 4, NULL, 1);

TRUNCATE TABLE picture RESTART IDENTITY CASCADE;
INSERT INTO picture (PlantId, PictureLink) VALUES
(1, 'https://upload.wikimedia.org/wikipedia/commons/thumb/8/8e/Illustration_Betula_pendula_very_clean.jpg/960px-Illustration_Betula_pendula_very_clean.jpg'),
(1, 'https://upload.wikimedia.org/wikipedia/commons/thumb/1/19/B._pendula%2C_Koivu_Birch_end_of_Sept.jpg/960px-B._pendula%2C_Koivu_Birch_end_of_Sept.jpg'),
(2, 'https://upload.wikimedia.org/wikipedia/commons/0/0b/Pinus_sylvestris_-_Köhler–s_Medizinal-Pflanzen-106.jpg'),
(2, 'https://upload.wikimedia.org/wikipedia/commons/thumb/8/88/Pinus_sylvestris_branch.jpg/960px-Pinus_sylvestris_branch.jpg'),
(2, 'https://upload.wikimedia.org/wikipedia/commons/thumb/4/47/Sosna_na_Sokolicy_1_wykadrowane.JPG/960px-Sosna_na_Sokolicy_1_wykadrowane.JPG'),
(3, 'https://upload.wikimedia.org/wikipedia/commons/thumb/b/b2/Taraxacum_officinale_-_Köhler–s_Medizinal-Pflanzen-135.jpg/960px-Taraxacum_officinale_-_Köhler–s_Medizinal-Pflanzen-135.jpg'),
(3, 'https://upload.wikimedia.org/wikipedia/commons/thumb/e/ee/Smetanka_lékařská.JPG/960px-Smetanka_lékařská.JPG'),
(3, 'https://upload.wikimedia.org/wikipedia/commons/thumb/4/4b/Löwenzahnblüte_im_Frühling0003.JPG/960px-Löwenzahnblüte_im_Frühling0003.JPG'),
(4, 'https://upload.wikimedia.org/wikipedia/commons/thumb/2/2e/Aloe_vera%2C_Jardín_Botánico%2C_Múnich%2C_Alemania_2012-04-21%2C_DD_01.JPG/960px-Aloe_vera%2C_Jardín_Botánico%2C_Múnich%2C_Alemania_2012-04-21%2C_DD_01.JPG'),
(4, 'https://upload.wikimedia.org/wikipedia/commons/thumb/b/b7/Fuerteventura_Aloe_Vera.jpg/960px-Fuerteventura_Aloe_Vera.jpg'),
(4, 'https://klubpodaloesem.pl/wp-content/uploads/2020/06/botanical-drawings-botanical-illustration.jpg'),
(5, 'https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Cantharellus_cibarius_20090717-02.jpg/960px-Cantharellus_cibarius_20090717-02.jpg'),
(5, 'https://cudnapolska.pl/wp-content/uploads/2024/04/Pieprznik-ametystowy-przekroj.jpg'),
(6, 'https://upload.wikimedia.org/wikipedia/commons/thumb/3/32/Amanita_muscaria_3_vliegenzwammen_op_rij.jpg/960px-Amanita_muscaria_3_vliegenzwammen_op_rij.jpg'),
(6, 'https://upload.wikimedia.org/wikipedia/commons/thumb/f/f5/Amanita_muscaria_section_1_WF_orig.jpg/960px-Amanita_muscaria_section_1_WF_orig.jpg'),
(7, 'https://upload.wikimedia.org/wikipedia/commons/thumb/c/c6/Agaricus_bisporus_G4.JPG/960px-Agaricus_bisporus_G4.JPG'),
(7, 'https://us.123rf.com/450wm/kirpmun/kirpmun1805/kirpmun180500019/100729700-champignon-mushroom-hand-drawn-sketch-illustration.jpg'),
(8, 'https://upload.wikimedia.org/wikipedia/commons/thumb/1/16/Brennnessel_1.JPG/960px-Brennnessel_1.JPG'),
(8, 'https://upload.wikimedia.org/wikipedia/commons/thumb/5/59/Urtica_dioica_kz17.jpg/960px-Urtica_dioica_kz17.jpg'),
(9, 'https://upload.wikimedia.org/wikipedia/commons/4/49/Mentha_×_piperita_-_Köhler–s_Medizinal-Pflanzen-095.jpg'),
(9, 'https://upload.wikimedia.org/wikipedia/commons/0/0d/Mentha-piperita.JPG'),
(9, 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSt-87bSr1CdFM2OZkHgr0o1FuWKZ4eNXt-YlrS3mGetIxWrX9gF8x_vNP2cZQRRIupAiDeq8Des3jfO7PS2hegbO-wl2BrPPcO4xIQJBU&s=10'),
(10, 'https://upload.wikimedia.org/wikipedia/commons/thumb/c/c7/Matricaria_recutita_-_Köhler–s_Medizinal-Pflanzen-091.jpg/960px-Matricaria_recutita_-_Köhler–s_Medizinal-Pflanzen-091.jpg'),
(10, 'https://upload.wikimedia.org/wikipedia/commons/thumb/5/5f/Atlas_roslin_pl_Rumianek_pospolity_2097_7384.jpg/960px-Atlas_roslin_pl_Rumianek_pospolity_2097_7384.jpg'),
(3, 'https://images.immediate.co.uk/production/volatile/sites/63/2024/08/ff8e4324ecdc5d98c32d405ec76cf431b1550108-a8c902d.jpeg?quality=90&resize=800,534');

TRUNCATE TABLE PRODUCTTYPE RESTART IDENTITY CASCADE;
INSERT INTO producttype (Name) VALUES
('Infusion'),
('Decoction'),
('Tincture'),
('Ointment'),
('Extract'),
('Syrup'),
('Tea blend'),
('Powder'),
('Juice'),
('Compress');

TRUNCATE TABLE PRODUCT RESTART IDENTITY CASCADE;
INSERT INTO product (Name, Recipe, HealthPropertyId, Contraindication, ProductTypeId) VALUES
('Dandelion syrup', 'Boil dandelion flowers in water, strain, add sugar and lemon juice, simmer until thick.', 2, 'allergy to Asteraceae plants', 6),
('Pine shoot syrup', 'Layer young pine shoots with sugar, leave for several weeks until syrup forms, strain.', 12, 'asthma (in some cases), allergy', 6),
('Chamomile infusion', 'Pour hot water over dried chamomile flowers, steep for 10 minutes', 9, 'allergy to chamomile', 1),
('Nettle juice', 'Blend fresh nettle leaves with water, strain the liquid', 11, 'kidney disorders, pregnancy', 9),
('Mint tea blend', 'Mix dried mint leaves with other herbs, pour hot water and steep', 7, 'gastric reflux (in excess)', 7);

TRUNCATE TABLE PLANT_PRODUCT RESTART IDENTITY CASCADE;
INSERT INTO plant_product (PlantsId, ProductsId) VALUES
(3, 1),
(2, 2),
(10, 3),
(8, 4),
(9, 5);

-- koniec pliku