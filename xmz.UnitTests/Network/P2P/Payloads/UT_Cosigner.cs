using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;

namespace Neo.UnitTests.Network.P2P.Payloads
{
    [TestClass]
    public class UT_Cosigner
    {
        [TestMethod]
        public void Serialize_Deserialize_Global()
        {
            var attr = new Cosigner()
            {
                Scopes = WitnessScope.Global,
                Account = UInt160.Zero
            };

            var hex = "000000000000000000000000000000000000000000";
            attr.ToArray().ToHexString().Should().Be(hex);

            var copy = hex.HexToBytes().AsSerializable<Cosigner>();

            Assert.AreEqual(attr.Scopes, copy.Scopes);
            Assert.AreEqual(attr.Account, copy.Account);
        }

        [TestMethod]
        public void Serialize_Deserialize_CalledByEntry()
        {
            var attr = new Cosigner()
            {
                Scopes = WitnessScope.CalledByEntry,
                Account = UInt160.Zero
            };

            var hex = "000000000000000000000000000000000000000001";
            attr.ToArray().ToHexString().Should().Be(hex);

            var copy = hex.HexToBytes().AsSerializable<Cosigner>();

            Assert.AreEqual(attr.Scopes, copy.Scopes);
            Assert.AreEqual(attr.Account, copy.Account);
        }

        [TestMethod]
        public void Serialize_Deserialize_CustomContracts()
        {
            var attr = new Cosigner()
            {
                Scopes = WitnessScope.CustomContracts,
                AllowedContracts = new[] { UInt160.Zero },
                Account = UInt160.Zero
            };

            var hex = "000000000000000000000000000000000000000010010000000000000000000000000000000000000000";
            attr.ToArray().ToHexString().Should().Be(hex);

            var copy = hex.HexToBytes().AsSerializable<Cosigner>();

            Assert.AreEqual(attr.Scopes, copy.Scopes);
            CollectionAssert.AreEqual(attr.AllowedContracts, copy.AllowedContracts);
            Assert.AreEqual(attr.Account, copy.Account);
        }

        [TestMethod]
        public void Serialize_Deserialize_CustomGroups()
        {
            var attr = new Cosigner()
            {
                Scopes = WitnessScope.CustomGroups,
                AllowedGroups = new[] { ECPoint.Parse("03b209fd4f53a7170ea4444e0cb0a6bb6a53c2bd016926989cf85f9b0fba17a70c", ECCurve.Secp256r1) },
                Account = UInt160.Zero
            };

            var hex = "0000000000000000000000000000000000000000200103b209fd4f53a7170ea4444e0cb0a6bb6a53c2bd016926989cf85f9b0fba17a70c";
            attr.ToArray().ToHexString().Should().Be(hex);

            var copy = hex.HexToBytes().AsSerializable<Cosigner>();

            Assert.AreEqual(attr.Scopes, copy.Scopes);
            CollectionAssert.AreEqual(attr.AllowedGroups, copy.AllowedGroups);
            Assert.AreEqual(attr.Account, copy.Account);
        }

        [TestMethod]
        public void Json_Global()
        {
            var attr = new Cosigner()
            {
                Scopes = WitnessScope.Global,
                Account = UInt160.Zero
            };

            var json = "{\"account\":\"0x0000000000000000000000000000000000000000\",\"scopes\":\"Global\"}";
            attr.ToJson().ToString().Should().Be(json);

            var copy = Cosigner.FromJson(JObject.Parse(json));

            Assert.AreEqual(attr.Scopes, copy.Scopes);
            Assert.AreEqual(attr.Account, copy.Account);
        }

        [TestMethod]
        public void Json_CalledByEntry()
        {
            var attr = new Cosigner()
            {
                Scopes = WitnessScope.CalledByEntry,
                Account = UInt160.Zero
            };

            var json = "{\"account\":\"0x0000000000000000000000000000000000000000\",\"scopes\":\"CalledByEntry\"}";
            attr.ToJson().ToString().Should().Be(json);

            var copy = Cosigner.FromJson(JObject.Parse(json));

            Assert.AreEqual(attr.Scopes, copy.Scopes);
            Assert.AreEqual(attr.Account, copy.Account);
        }

        [TestMethod]
        public void Json_CustomContracts()
        {
            var attr = new Cosigner()
            {
                Scopes = WitnessScope.CustomContracts,
                AllowedContracts = new[] { UInt160.Zero },
                Account = UInt160.Zero
            };

            var json = "{\"account\":\"0x0000000000000000000000000000000000000000\",\"scopes\":\"CustomContracts\",\"allowedContracts\":[\"0x0000000000000000000000000000000000000000\"]}";
            attr.ToJson().ToString().Should().Be(json);

            var copy = Cosigner.FromJson(JObject.Parse(json));

            Assert.AreEqual(attr.Scopes, copy.Scopes);
            CollectionAssert.AreEqual(attr.AllowedContracts, copy.AllowedContracts);
            Assert.AreEqual(attr.Account, copy.Account);
        }

        [TestMethod]
        public void Json_CustomGroups()
        {
            var attr = new Cosigner()
            {
                Scopes = WitnessScope.CustomGroups,
                AllowedGroups = new[] { ECPoint.Parse("03b209fd4f53a7170ea4444e0cb0a6bb6a53c2bd016926989cf85f9b0fba17a70c", ECCurve.Secp256r1) },
                Account = UInt160.Zero
            };

            var json = "{\"account\":\"0x0000000000000000000000000000000000000000\",\"scopes\":\"CustomGroups\",\"allowedGroups\":[\"03b209fd4f53a7170ea4444e0cb0a6bb6a53c2bd016926989cf85f9b0fba17a70c\"]}";
            attr.ToJson().ToString().Should().Be(json);

            var copy = Cosigner.FromJson(JObject.Parse(json));

            Assert.AreEqual(attr.Scopes, copy.Scopes);
            CollectionAssert.AreEqual(attr.AllowedGroups, copy.AllowedGroups);
            Assert.AreEqual(attr.Account, copy.Account);
        }
    }
}
