using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsUnitairesPourServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestsUnitairesPourServices.Data;
using TestsUnitairesPourServices.Models;
using TestsUnitairesPourServices.Exceptions;

namespace TestsUnitairesPourServices.Services.Tests
{
    [TestClass()]
    public class CatsServiceTests
    {
        DbContextOptions<ApplicationDBContext> options;
        public CatsServiceTests() 
        {
            options = new DbContextOptionsBuilder<ApplicationDBContext>()
            // TODO il faut installer la dépendance Microsoft.EntityFrameworkCore.InMemory
            .UseInMemoryDatabase(databaseName: "CatsService")
            .UseLazyLoadingProxies(true) // Active le lazy loading
            .Options;

        }

        [TestInitialize]
        public void Init()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);

            House[] houses = new House[]
            {
                new House
                {
                    Id = 1,
                    Address = "Rue du chat",
                    OwnerName = "Nicolas",
                    Cats = new List<Cat>{}
                },
                new House
                {
                    Id = 2,
                    Address = "Avenue du chat",
                    OwnerName = "Alexandre",
                    Cats = new List<Cat>{}
                }
            };

            Cat[] cats = new Cat[]
            {
                new Cat
                {
                    Id = 1,
                    Name = "Pandachat",
                    Age = 11,
                    House = houses[0]
                },
                new Cat
                {
                    Id = 2,
                    Name = "Toutoune",
                    Age = 15,
                    House = null
                }
            };
            db.AddRange(cats);
            db.AddRange(houses);
            db.SaveChanges();
        }

        [TestCleanup]
        public void Dispose()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            db.Cat.RemoveRange(db.Cat);
            db.House.RemoveRange(db.House);
            db.SaveChanges();
        }

        [TestMethod()]
        public void MoveTestExceptionWildCat()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            CatsService service = new CatsService(db);

            List<House> houses = db.House.ToList();
            House house1 = houses[0];
            House house2 = houses[1];

            Exception e = Assert.ThrowsException<WildCatException>(() => service.Move(2, house1, house2));
            Assert.AreEqual("On n'apprivoise pas les chats sauvages", e.Message);
        }

        [TestMethod()]
        public void MoveTestExceptionDontSteal()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            CatsService service = new CatsService(db);

            List<House> houses = db.House.ToList();
            House house1 = db.House.Find(1);
            House house2 = db.House.Find(2);

            Exception e = Assert.ThrowsException<DontStealMyCatException>(() => service.Move(1, house2, house1));
            Assert.AreEqual("Touche pas à mon chat!", e.Message);
        }

        [TestMethod()]
        public void Move()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            CatsService service = new CatsService(db);

            List<House> houses = db.House.ToList();
            House house1 = db.House.Find(1);
            House house2 = db.House.Find(2);

            Assert.IsNotNull(service.Move(1, house1, house2));

        }

        [TestMethod()]
        public void MoveChatInconnu()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            CatsService service = new CatsService(db);

            List<House> houses = db.House.ToList();
            House house1 = db.House.Find(1);
            House house2 = db.House.Find(2);

            Assert.IsNull(service.Move(3, house1, house2));

        }

        [TestMethod()]
        public void MoveChatSansMaison()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            CatsService service = new CatsService(db);

            List<House> houses = db.House.ToList();
            House house1 = db.House.Find(1);
            House house2 = db.House.Find(2);

            Assert.ThrowsException<WildCatException>(() => service.Move(2,house1,house2));

        }

        [TestMethod()]
        public void MoveChatMauvaiseMaison()
        {
            using ApplicationDBContext db = new ApplicationDBContext(options);
            CatsService service = new CatsService(db);

            List<House> houses = db.House.ToList();
            House house1 = db.House.Find(1);
            House house2 = db.House.Find(2);

            Assert.ThrowsException<WildCatException>(() => service.Move(2, house1, house2));

        }
    }
}