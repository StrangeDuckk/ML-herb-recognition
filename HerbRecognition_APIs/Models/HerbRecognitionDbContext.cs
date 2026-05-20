using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HerbRecognition_APIs.Models;

public partial class HerbRecognitionDbContext : DbContext
{
    public HerbRecognitionDbContext()
    {
    }

    public HerbRecognitionDbContext(DbContextOptions<HerbRecognitionDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activesubstance> Activesubstances { get; set; }

    public virtual DbSet<Color> Colors { get; set; }

    public virtual DbSet<Disease> Diseases { get; set; }

    public virtual DbSet<Flavour> Flavours { get; set; }

    public virtual DbSet<Flower> Flowers { get; set; }

    public virtual DbSet<Fruit> Fruits { get; set; }

    public virtual DbSet<Hat> Hats { get; set; }

    public virtual DbSet<Healthproperty> Healthproperties { get; set; }

    public virtual DbSet<Leaf> Leaves { get; set; }

    public virtual DbSet<Occurance> Occurances { get; set; }

    public virtual DbSet<Picture> Pictures { get; set; }

    public virtual DbSet<Plant> Plants { get; set; }

    public virtual DbSet<PlantProduct> PlantProducts { get; set; }

    public virtual DbSet<Planttype> Planttypes { get; set; }

    public virtual DbSet<Poisonability> Poisonabilities { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Producttype> Producttypes { get; set; }

    public virtual DbSet<Root> Roots { get; set; }

    public virtual DbSet<Sap> Saps { get; set; }

    public virtual DbSet<Shape> Shapes { get; set; }

    public virtual DbSet<Stalk> Stalks { get; set; }

    public virtual DbSet<Surface> Surfaces { get; set; }

    public virtual DbSet<Thickness> Thicknesses { get; set; }

    public virtual DbSet<Userinput> Userinputs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activesubstance>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("activesubstance_pk");

            entity.ToTable("activesubstance");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");

            entity.Property(e => e.Activesubstance1)
                .IsRequired()
                .HasColumnName("activesubstance");
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("color_pk");

            entity.ToTable("color");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id")
                .IsRequired();

            entity.Property(e => e.Color1)
                .HasMaxLength(50)
                .HasColumnName("color")
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.B)
                .HasColumnName("b")
                .IsRequired();

            entity.Property(e => e.G)
                .HasColumnName("g")
                .IsRequired();

            entity.Property(e => e.R)
                .HasColumnName("r")
                .IsRequired();
        });

        modelBuilder.Entity<Disease>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("disease_pk");

            entity.ToTable("disease");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .HasColumnName("name");
            entity.Property(e => e.Symptoms).HasColumnName("symptoms");
        });

        modelBuilder.Entity<Flavour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("flavour_pk");

            entity.ToTable("flavour");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Flavour1)
                .HasMaxLength(50)
                .HasColumnName("flavour");
        });

        modelBuilder.Entity<Flower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("flower_pk");

            entity.ToTable("flower");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Colorid).HasColumnName("colorid");
            entity.Property(e => e.Flavourid).HasColumnName("flavourid");
            entity.Property(e => e.Scentpower).HasColumnName("scentpower");
            entity.Property(e => e.Shapeid).HasColumnName("shapeid");
            entity.Property(e => e.Sizeincm)
                .HasPrecision(5, 2)
                .HasColumnName("sizeincm");

            entity.HasOne(d => d.Color).WithMany(p => p.Flowers)
                .HasForeignKey(d => d.Colorid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("flower_dictcolor");

            entity.HasOne(d => d.Flavour).WithMany(p => p.Flowers)
                .HasForeignKey(d => d.Flavourid)
                .HasConstraintName("flowers_flavours");

            entity.HasOne(d => d.Shape).WithMany(p => p.Flowers)
                .HasForeignKey(d => d.Shapeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("flower_dictshape");
        });

        modelBuilder.Entity<Fruit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("fruit_pk");

            entity.ToTable("fruit");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Colorid).HasColumnName("colorid");
            entity.Property(e => e.Flavourid).HasColumnName("flavourid");
            entity.Property(e => e.Shapeid).HasColumnName("shapeid");
            entity.Property(e => e.Surfaceid).HasColumnName("surfaceid");
            entity.Property(e => e.Thicknessid).HasColumnName("thicknessid");

            entity.HasOne(d => d.Color).WithMany(p => p.Fruits)
                .HasForeignKey(d => d.Colorid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fruit_dictcolor");

            entity.HasOne(d => d.Flavour).WithMany(p => p.Fruits)
                .HasForeignKey(d => d.Flavourid)
                .HasConstraintName("fruit_flavor");

            entity.HasOne(d => d.Shape).WithMany(p => p.Fruits)
                .HasForeignKey(d => d.Shapeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fruit_dictshape");

            entity.HasOne(d => d.Surface).WithMany(p => p.Fruits)
                .HasForeignKey(d => d.Surfaceid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fruit_dictsurface");

            entity.HasOne(d => d.Thickness).WithMany(p => p.Fruits)
                .HasForeignKey(d => d.Thicknessid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fruit_thickness");
        });

        modelBuilder.Entity<Hat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hat_pk");

            entity.ToTable("hat");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Colorid).HasColumnName("colorid");
            entity.Property(e => e.Hasgills).HasColumnName("hasgills");
            entity.Property(e => e.Hasspots).HasColumnName("hasspots");
            entity.Property(e => e.Shapeid).HasColumnName("shapeid");
            entity.Property(e => e.Surfaceid).HasColumnName("surfaceid");
            entity.Property(e => e.Thicknessid).HasColumnName("thicknessid");

            entity.HasOne(d => d.Color).WithMany(p => p.Hats)
                .HasForeignKey(d => d.Colorid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hat_dictcolor");

            entity.HasOne(d => d.Shape).WithMany(p => p.Hats)
                .HasForeignKey(d => d.Shapeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hat_dictshape");

            entity.HasOne(d => d.Surface).WithMany(p => p.Hats)
                .HasForeignKey(d => d.Surfaceid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hat_dictsurface");

            entity.HasOne(d => d.Thickness).WithMany(p => p.Hats)
                .HasForeignKey(d => d.Thicknessid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hat_thickness");
        });

        modelBuilder.Entity<Healthproperty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("healthproperty_pk");

            entity.ToTable("healthproperty");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Activesubstanceid).HasColumnName("activesubstanceid");
            entity.Property(e => e.Diseaseid).HasColumnName("diseaseid");
            entity.Property(e => e.Property)
                .HasMaxLength(300)
                .HasColumnName("property");

            entity.HasOne(d => d.Activesubstance).WithMany(p => p.Healthproperties)
                .HasForeignKey(d => d.Activesubstanceid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("healthproperties_activesubstances");

            entity.HasOne(d => d.Disease).WithMany(p => p.Healthproperties)
                .HasForeignKey(d => d.Diseaseid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("healthproperties_diseases");
        });

        modelBuilder.Entity<Leaf>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("leaf_pk");

            entity.ToTable("leaf");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Flavourid).HasColumnName("flavourid");
            entity.Property(e => e.Holes).HasColumnName("holes");
            entity.Property(e => e.Leafcolorid).HasColumnName("leafcolorid");
            entity.Property(e => e.Leaflength)
                .HasPrecision(3)
                .HasColumnName("leaflength");
            entity.Property(e => e.Leafshapeid).HasColumnName("leafshapeid");
            entity.Property(e => e.Spots).HasColumnName("spots");
            entity.Property(e => e.Stripes).HasColumnName("stripes");
            entity.Property(e => e.Surfaceid).HasColumnName("surfaceid");
            entity.Property(e => e.Thicknessid).HasColumnName("thicknessid");

            entity.HasOne(d => d.Flavour).WithMany(p => p.Leaves)
                .HasForeignKey(d => d.Flavourid)
                .HasConstraintName("leafs_flavours");

            entity.HasOne(d => d.Leafcolor).WithMany(p => p.Leaves)
                .HasForeignKey(d => d.Leafcolorid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("leaf_leafcolor");

            entity.HasOne(d => d.Leafshape).WithMany(p => p.Leaves)
                .HasForeignKey(d => d.Leafshapeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("leaf_leafshape");

            entity.HasOne(d => d.Surface).WithMany(p => p.Leaves)
                .HasForeignKey(d => d.Surfaceid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("leaf_dictsurface");

            entity.HasOne(d => d.Thickness).WithMany(p => p.Leaves)
                .HasForeignKey(d => d.Thicknessid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("leaf_thickness");
        });

        modelBuilder.Entity<Occurance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("occurance_pk");

            entity.ToTable("occurance");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Occurance1)
                .HasMaxLength(150)
                .HasColumnName("occurance");
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("picture_pk");

            entity.ToTable("picture");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Picturelink).HasColumnName("picturelink");
            entity.Property(e => e.Plantid).HasColumnName("plantid");

            entity.HasOne(d => d.Plant).WithMany(p => p.Pictures)
                .HasForeignKey(d => d.Plantid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pictures_plants");
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("plant_pk");

            entity.ToTable("plant");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Flowerid).HasColumnName("flowerid");
            entity.Property(e => e.Fruitid).HasColumnName("fruitid");
            entity.Property(e => e.Hatid).HasColumnName("hatid");
            entity.Property(e => e.Latinname)
                .HasMaxLength(50)
                .HasColumnName("latinname");
            entity.Property(e => e.Leafid).HasColumnName("leafid");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Occuranceid).HasColumnName("occuranceid");
            entity.Property(e => e.Planttypeid).HasColumnName("planttypeid");
            entity.Property(e => e.Poisonabilityid).HasColumnName("poisonabilityid");
            entity.Property(e => e.Polishname)
                .HasMaxLength(50)
                .HasColumnName("polishname");
            entity.Property(e => e.Rootid).HasColumnName("rootid");
            entity.Property(e => e.Sapid).HasColumnName("sapid");
            entity.Property(e => e.Similarplantsid).HasColumnName("similarplantsid");
            entity.Property(e => e.Stalkid).HasColumnName("stalkid");
            entity.Property(e => e.Subriquet)
                .HasMaxLength(50)
                .HasColumnName("subriquet");

            entity.HasOne(d => d.Flower).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Flowerid)
                .HasConstraintName("plants_flower");

            entity.HasOne(d => d.Fruit).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Fruitid)
                .HasConstraintName("plants_fruit");

            entity.HasOne(d => d.Hat).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Hatid)
                .HasConstraintName("plants_hat");

            entity.HasOne(d => d.Leaf).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Leafid)
                .HasConstraintName("plants_leaf");

            entity.HasOne(d => d.Occurance).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Occuranceid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_occurance");

            entity.HasOne(d => d.Planttype).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Planttypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("plants_type");

            entity.HasOne(d => d.Poisonability).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Poisonabilityid)
                .HasConstraintName("plants_poisonability");

            entity.HasOne(d => d.Root).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Rootid)
                .HasConstraintName("entity_root");

            entity.HasOne(d => d.Sap).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Sapid)
                .HasConstraintName("entity_sap");

            entity.HasOne(d => d.Similarplants).WithMany(p => p.InverseSimilarplants)
                .HasForeignKey(d => d.Similarplantsid)
                .HasConstraintName("plants_plants");

            entity.HasOne(d => d.Stalk).WithMany(p => p.Plants)
                .HasForeignKey(d => d.Stalkid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_stalk");
        });

        modelBuilder.Entity<PlantProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("plant_product_pk");

            entity.ToTable("plant_product");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Plantsid).HasColumnName("plantsid");
            entity.Property(e => e.Productsid).HasColumnName("productsid");

            entity.HasOne(d => d.Plants).WithMany(p => p.PlantProducts)
                .HasForeignKey(d => d.Plantsid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("plants_properties_plants");

            entity.HasOne(d => d.Products).WithMany(p => p.PlantProducts)
                .HasForeignKey(d => d.Productsid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("plants_properties_products");
        });

        modelBuilder.Entity<Planttype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("planttype_pk");

            entity.ToTable("planttype");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Poisonability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("poisonability_pk");

            entity.ToTable("poisonability");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_pk");

            entity.ToTable("product");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Contraindication)
                .HasMaxLength(100)
                .HasColumnName("contraindication");
            entity.Property(e => e.Healthpropertyid).HasColumnName("healthpropertyid");
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .HasColumnName("name");
            entity.Property(e => e.Producttypeid).HasColumnName("producttypeid");
            entity.Property(e => e.Recipe).HasColumnName("recipe");

            entity.HasOne(d => d.Healthproperty).WithMany(p => p.Products)
                .HasForeignKey(d => d.Healthpropertyid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_healthproperties");

            entity.HasOne(d => d.Producttype).WithMany(p => p.Products)
                .HasForeignKey(d => d.Producttypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_producttypes");
        });

        modelBuilder.Entity<Producttype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("producttype_pk");

            entity.ToTable("producttype");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Root>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("root_pk");

            entity.ToTable("root");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Colorid).HasColumnName("colorid");
            entity.Property(e => e.Surfaceid).HasColumnName("surfaceid");
            entity.Property(e => e.Thicknessid).HasColumnName("thicknessid");

            entity.HasOne(d => d.Color).WithMany(p => p.Roots)
                .HasForeignKey(d => d.Colorid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("root_dictcolor");

            entity.HasOne(d => d.Surface).WithMany(p => p.Roots)
                .HasForeignKey(d => d.Surfaceid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("root_dictsurface");

            entity.HasOne(d => d.Thickness).WithMany(p => p.Roots)
                .HasForeignKey(d => d.Thicknessid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("root_thickness");
        });

        modelBuilder.Entity<Sap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sap_pk");

            entity.ToTable("sap");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Colorid).HasColumnName("colorid");
            entity.Property(e => e.Leavesstains).HasColumnName("leavesstains");
            entity.Property(e => e.Sticky).HasColumnName("sticky");

            entity.HasOne(d => d.Color).WithMany(p => p.Saps)
                .HasForeignKey(d => d.Colorid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sap_dictcolor");
        });

        modelBuilder.Entity<Shape>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shape_pk");

            entity.ToTable("shape");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Shape1)
                .HasMaxLength(50)
                .HasColumnName("shape");
        });

        modelBuilder.Entity<Stalk>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stalk_pk");

            entity.ToTable("stalk");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Colorid).HasColumnName("colorid");
            entity.Property(e => e.Shapeid).HasColumnName("shapeid");
            entity.Property(e => e.Surfaceid).HasColumnName("surfaceid");

            entity.HasOne(d => d.Color).WithMany(p => p.Stalks)
                .HasForeignKey(d => d.Colorid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stalk_dictcolor");

            entity.HasOne(d => d.Shape).WithMany(p => p.Stalks)
                .HasForeignKey(d => d.Shapeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stalk_dictshape");

            entity.HasOne(d => d.Surface).WithMany(p => p.Stalks)
                .HasForeignKey(d => d.Surfaceid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stalk_dictsurface");
        });

        modelBuilder.Entity<Surface>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("surface_pk");

            entity.ToTable("surface");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Surface1)
                .HasMaxLength(50)
                .HasColumnName("surface");
        });

        modelBuilder.Entity<Thickness>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("thicknesse_pk");

            entity.ToTable("thickness");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Thickness1)
                .HasMaxLength(50)
                .HasColumnName("thickness");
        });

        modelBuilder.Entity<Userinput>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("userinput_pkey");

            entity.ToTable("userinput");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Inputdata)
                .HasColumnType("jsonb")
                .HasColumnName("inputdata");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
