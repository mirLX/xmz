using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.Wallets;
using System;

namespace Neo.UnitTests.SmartContract
{
    [TestClass]
    public class UT_ContractParameterContext
    {
        private static Contract contract;
        private static KeyPair key;

        [ClassInitialize]
        public static void ClassSetUp(TestContext context)
        {
            if (contract == null)
            {
                byte[] privateKey = new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                                                 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                                                 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                                                 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 };
                key = new KeyPair(privateKey);
                contract = Contract.CreateSignatureContract(key.PublicKey);
            }
            TestBlockchain.InitializeMockNeoSystem();
        }

        [TestMethod]
        public void TestGetComplete()
        {
            Transaction tx = TestUtils.GetTransaction();
            tx.Sender = UInt160.Parse("0x1a2791a63139294337863c7d822d17454876977c");
            var context = new ContractParametersContext(tx);
            context.Completed.Should().BeFalse();
        }

        [TestMethod]
        public void TestToString()
        {
            Transaction tx = TestUtils.GetTransaction();
            tx.Sender = UInt160.Parse("0x1a2791a63139294337863c7d822d17454876977c");
            var context = new ContractParametersContext(tx);
            context.Add(contract, 0, new byte[] { 0x01 });
            string str = context.ToString();
            str.Should().Be(@"{""type"":""Neo.Network.P2P.Payloads.Transaction"",""hex"":""AAAAAAB8l3ZIRRctgn08hjdDKTkxppEnGgAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAA=="",""items"":{""0x1a2791a63139294337863c7d822d17454876977c"":{""script"":""IQJv8DuUkkHOHa3UNRnmlg4KhbQaaaBcMoEDqivOFZTKFlBoCpBq1A=="",""parameters"":[{""type"":""Signature"",""value"":""AQ==""}]}}}");
        }

        [TestMethod]
        public void TestParse()
        {
            var ret = ContractParametersContext.Parse("{\"type\":\"Neo.Network.P2P.Payloads.Transaction\",\"hex\":\"AAAAAADyd\\/EUQDWkOJf5\\u002BhFSWOrAFa3KvgAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAA==\",\"items\":{\"0xbecaad15c0ea585211faf99738a4354014f177f2\":{\"script\":\"IQJv8DuUkkHOHa3UNRnmlg4KhbQaaaBcMoEDqivOFZTKFmh0dHaq\",\"parameters\":[{\"type\":\"Signature\",\"value\":\"AQ==\"}]}}}");
            ret.ScriptHashes[0].ToString().Should().Be("0xbecaad15c0ea585211faf99738a4354014f177f2");
            ((Transaction)ret.Verifiable).Script.ToHexString().Should().Be(new byte[1].ToHexString());
        }

        [TestMethod]
        public void TestFromJson()
        {
            Action action = () => ContractParametersContext.Parse("{\"type\":\"wrongType\",\"hex\":\"00000000007c97764845172d827d3c863743293931a691271a0000000000000000000000000000000000000000000100\",\"items\":{\"0x1a2791a63139294337863c7d822d17454876977c\":{\"script\":\"21026ff03b949241ce1dadd43519e6960e0a85b41a69a05c328103aa2bce1594ca1650680a906ad4\",\"parameters\":[{\"type\":\"Signature\",\"value\":\"01\"}]}}}");
            action.Should().Throw<FormatException>();
        }

        [TestMethod]
        public void TestAdd()
        {
            Transaction tx = TestUtils.GetTransaction();
            var context1 = new ContractParametersContext(tx);
            context1.Add(contract, 0, new byte[] { 0x01 }).Should().BeFalse();

            tx.Sender = UInt160.Parse("0x1a2791a63139294337863c7d822d17454876977c");
            var context2 = new ContractParametersContext(tx);
            context2.Add(contract, 0, new byte[] { 0x01 }).Should().BeTrue();
            //test repeatlly createItem
            context2.Add(contract, 0, new byte[] { 0x01 }).Should().BeTrue();
        }

        [TestMethod]
        public void TestGetParameter()
        {
            Transaction tx = TestUtils.GetTransaction();
            tx.Sender = UInt160.Parse("0x1a2791a63139294337863c7d822d17454876977c");
            var context = new ContractParametersContext(tx);
            context.GetParameter(tx.Sender, 0).Should().BeNull();

            context.Add(contract, 0, new byte[] { 0x01 });
            var ret = context.GetParameter(tx.Sender, 0);
            ((byte[])ret.Value).ToHexString().Should().Be(new byte[] { 0x01 }.ToHexString());
        }

        [TestMethod]
        public void TestGetWitnesses()
        {
            Transaction tx = TestUtils.GetTransaction();
            tx.Sender = UInt160.Parse("0x1a2791a63139294337863c7d822d17454876977c");
            var context = new ContractParametersContext(tx);
            context.Add(contract, 0, new byte[] { 0x01 });
            Witness[] witnesses = context.GetWitnesses();
            witnesses.Length.Should().Be(1);
            witnesses[0].InvocationScript.ToHexString().Should().Be(new byte[] { 0x01, 0x01 }.ToHexString());
            witnesses[0].VerificationScript.ToHexString().Should().Be(contract.Script.ToHexString());
        }

        [TestMethod]
        public void TestAddSignature()
        {
            Transaction tx = TestUtils.GetTransaction();
            var singleSender = UInt160.Parse("0x1a2791a63139294337863c7d822d17454876977c");
            tx.Sender = singleSender;

            //singleSign

            var context = new ContractParametersContext(tx);
            context.AddSignature(contract, key.PublicKey, new byte[] { 0x01 }).Should().BeTrue();

            var contract1 = Contract.CreateSignatureContract(key.PublicKey);
            contract1.ParameterList = new ContractParameterType[0];
            context = new ContractParametersContext(tx);
            context.AddSignature(contract1, key.PublicKey, new byte[] { 0x01 }).Should().BeFalse();

            contract1.ParameterList = new[] { ContractParameterType.Signature, ContractParameterType.Signature };
            Action action1 = () => context.AddSignature(contract1, key.PublicKey, new byte[] { 0x01 });
            action1.Should().Throw<NotSupportedException>();

            //multiSign

            byte[] privateKey2 = new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                                              0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                                              0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                                              0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x02 };
            var key2 = new KeyPair(privateKey2);
            var multiSignContract = Contract.CreateMultiSigContract(2,
                    new ECPoint[]
                    {
                        key.PublicKey,
                        key2.PublicKey
                    });
            var multiSender = UInt160.Parse("0xfc8b59f1a337dcc17b1a201d327a2081d41fac8d");
            tx.Sender = multiSender;
            context = new ContractParametersContext(tx);
            context.AddSignature(multiSignContract, key.PublicKey, new byte[] { 0x01 }).Should().BeTrue();
            context.AddSignature(multiSignContract, key2.PublicKey, new byte[] { 0x01 }).Should().BeTrue();

            tx.Sender = singleSender;
            context = new ContractParametersContext(tx);
            context.AddSignature(multiSignContract, key.PublicKey, new byte[] { 0x01 }).Should().BeFalse();

            tx.Sender = multiSender;
            context = new ContractParametersContext(tx);
            byte[] privateKey3 = new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                                              0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                                              0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                                              0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x03 };
            var key3 = new KeyPair(privateKey3);
            context.AddSignature(multiSignContract, key3.PublicKey, new byte[] { 0x01 }).Should().BeFalse();
        }
    }
}
