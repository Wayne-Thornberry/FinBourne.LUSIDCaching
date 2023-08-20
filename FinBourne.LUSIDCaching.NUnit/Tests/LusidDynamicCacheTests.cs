using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinBourne.LUSIDCaching.NUnit.Tests
{
    public class LusidDynamicCacheTests
    {
        [Test]
        public void BasicAddCachedObjectSuccess()
        {
            // Assemble
            const string Obj = "Value";

            // Act
            LusidDynamicCache.AddItem("Test", Obj);

            // Assert
            var item = LusidDynamicCache.GetItem("Test");
            Assert.NotNull(item);
            Assert.AreEqual("Value", item);
        }

        [Test]
        public void BasicAddCachedObjectDifferentValueSuccess()
        {
            // Assemble
            var x = 1;

            // Act
            LusidDynamicCache.AddItem("Test", x);

            // Assert
            var item = LusidDynamicCache.GetItem("Test");
            Assert.NotNull(item);
            Assert.AreEqual(x, item);
        }

        [Test]
        public void BasicAddCachedObjectConfiguredLimitSuccess()
        {
            //Assemble
            var limit = 2;

            // Act
            LusidDynamicCache.Configure(limit);

            // Assert
            Assert.AreEqual(limit, LusidDynamicCache.Capacity);
        }


        [Test]
        public void BasicAddCachedObjectUnderConfiguredLimitSuccess()
        {
            //Assemble
            var limit = -2;

            // Act
            LusidDynamicCache.Configure(limit);

            // Assert
            Assert.AreNotEqual(limit, LusidDynamicCache.Capacity);
        }


        [Test]
        public void BasicAddCachedObjectObjectDoesNotExistSuccess()
        {
            // Assemble

            // Act
            LusidDynamicCache.AddItem("Test", "Value");

            // Assert
            var item = LusidDynamicCache.GetItem("Test2");
            Assert.IsNull(item);
        }

        [Test]
        public void BasicAddMultipleCachedObjectObjectsSuccess()
        {
            // Assemble
            string[] keys = new string[LusidDynamicCache.Capacity];

            // Act
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = Guid.NewGuid().ToString();
                LusidDynamicCache.AddItem(keys[i], RandomString(30));
            }

            // Assert
            for (int i = 0; i < keys.Length; i++)
            {
                var item = LusidDynamicCache.GetItem(keys[i]);
                Assert.IsNotNull(item); 
            } 
        }

        [Test]
        public void BasicAddMoreThanCapacityCachedObjectObjectsSuccess()
        {
            // Assemble
            string[] keys = new string[LusidDynamicCache.Capacity + 1];

            // Setup the first item that will be overridden
            LusidDynamicCache.AddItem("First", RandomString(30));

            // Act
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = Guid.NewGuid().ToString();
                LusidDynamicCache.AddItem(keys[i], RandomString(30));
            }

            // Assert
            for (int i = 0; i < keys.Length; i++)
            {
                var item = LusidDynamicCache.GetItem(keys[i]);
                Assert.IsNotNull(item);
            }


            var item2 = LusidDynamicCache.GetItem("First");
            Assert.IsNull(item2);
        }



        [Test]
        public void BasicAddMoreThanCapacityCachedObjectObjectsConfiguredLimitSuccess()
        {
            // Assemble
            string[] keys = new string[3];
            LusidDynamicCache.Configure(2);

            // Setup the first item that will be overridden
            LusidDynamicCache.AddItem("First", RandomString(30));

            // Act
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = Guid.NewGuid().ToString();
                LusidDynamicCache.AddItem(keys[i], RandomString(30));
            }

            // Assert
            for (int i = 0; i < keys.Length; i++)
            {
                var item = LusidDynamicCache.GetItem(keys[i]);
                Assert.IsNotNull(item);
            }


            var item2 = LusidDynamicCache.GetItem("First");
            Assert.IsNull(item2);
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
