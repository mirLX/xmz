using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.IO;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.Wallets;
using Neo.Wallets.NEP6;
using System;
using System.Linq;

namespace Neo.UnitTests.Network.P2P.Payloads
{
    [TestClass]
    public class UT_Witness
    {
        Witness uut;

        [TestInitialize]
        public void TestSetup()
        {
            uut = new Witness();
        }

        [TestMethod]
        public void InvocationScript_Get()
        {
            uut.InvocationScript.Should().BeNull();
        }

        private Witness PrepareDummyWitness(int maxAccounts)
        {
            var address = new WalletAccount[maxAccounts];
            var wallets = new NEP6Wallet[maxAccounts];
            var walletsUnlocks = new IDisposable[maxAccounts];

            for (int x = 0; x < maxAccounts; x++)
            {
                wallets[x] = TestUtils.GenerateTestWallet();
                walletsUnlocks[x] = wallets[x].Unlock("123");
                address[x] = wallets[x].CreateAccount();
            }

            // Generate multisignature

            var multiSignContract = Contract.CreateMultiSigContract(maxAccounts, address.Select(a => a.GetKey().PublicKey).ToArray());

            for (int x = 0; x < maxAccounts; x++)
            {
                wallets[x].CreateAccount(multiSignContract, address[x].GetKey());
            }

            // Sign

            var data = new ContractParametersContext(new Transaction()
            {
                Cosigners = new Cosigner[0],
                Sender = multiSignContract.ScriptHash,
                Attributes = new TransactionAttribute[0],
                NetworkFee = 0,
                Nonce = 0,
                Script = new byte[0],
                SystemFee = 0,
                ValidUntilBlock = 0,
                Version = 0,
                Witnesses = new Witness[0]
            });

            for (int x = 0; x < maxAccounts; x++)
            {
                Assert.IsTrue(wallets[x].Sign(data));
            }

            Assert.IsTrue(data.Completed);
            return data.GetWitnesses()[0];
        }

        [TestMethod]
        public void MaxSize_OK()
        {
            var witness = PrepareDummyWitness(10);

            // Check max size

            witness.Size.Should().Be(1004);
            witness.InvocationScript.GetVarSize().Should().Be(653);
            witness.VerificationScript.GetVarSize().Should().Be(351);

            Assert.IsTrue(witness.Size <= 1024);

            var copy = witness.ToArray().AsSerializable<Witness>();

            CollectionAssert.AreEqual(witness.InvocationScript, copy.InvocationScript);
            CollectionAssert.AreEqual(witness.VerificationScript, copy.VerificationScript);
        }

        [TestMethod]
        public void MaxSize_Error()
        {
            var witness = PrepareDummyWitness(11);

            // Check max size

            Assert.IsTrue(witness.Size > 1024);
            Assert.ThrowsException<FormatException>(() => witness.ToArray().AsSerializable<Witness>());
        }

        [TestMethod]
        public void InvocationScript_Set()
        {
            byte[] dataArray = new byte[] { 0, 32, 32, 20, 32, 32 };
            uut.InvocationScript = dataArray;
            uut.InvocationScript.Length.Should().Be(6);
            Assert.AreEqual(uut.InvocationScript.ToHexString(), "002020142020");
        }

        private void SetupWitnessWithValues(Witness uut, int lenghtInvocation, int lengthVerification, out byte[] invocationScript, out byte[] verificationScript)
        {
            invocationScript = TestUtils.GetByteArray(lenghtInvocation, 0x20);
            verificationScript = TestUtils.GetByteArray(lengthVerification, 0x20);
            uut.InvocationScript = invocationScript;
            uut.VerificationScript = verificationScript;
        }

        [TestMethod]
        public void SizeWitness_Small_Arrary()
        {
            SetupWitnessWithValues(uut, 252, 253, out _, out _);

            uut.Size.Should().Be(509); // (1 + 252*1) + (1 + 2 + 253*1)
        }

        [TestMethod]
        public void SizeWitness_Large_Arrary()
        {
            SetupWitnessWithValues(uut, 65535, 65536, out _, out _);

            uut.Size.Should().Be(131079); // (1 + 2 + 65535*1) + (1 + 4 + 65536*1)
        }

        [TestMethod]
        public void ToJson()
        {
            SetupWitnessWithValues(uut, 2, 3, out _, out _);

            JObject json = uut.ToJson();
            Assert.IsTrue(json.ContainsProperty("invocation"));
            Assert.IsTrue(json.ContainsProperty("verification"));
            Assert.AreEqual(json["invocation"].AsString(), "ICA=");
            Assert.AreEqual(json["verification"].AsString(), "ICAg");
        }
    }
}
