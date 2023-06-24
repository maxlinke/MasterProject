using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using MasterProject.MachineLearning;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace MasterProject.Tests {

    [TestFixture]
    public class GenericIndividualsTests {

        public class TestParams : IParameterListConvertible<float>, IParameterRangeProvider<float> {

            public static float paramRangeMin { get; set; }
            public static float paramRangeMax { get; set; }

            public const int DEFAULT_PARAM_COUNT = 4;

            public float[] parameters { get; set; } = new float[DEFAULT_PARAM_COUNT];

            void IParameterListConvertible<float>.ApplyParameterList (IReadOnlyList<float> parameterList) => parameters = parameterList.ToArray();

            IReadOnlyList<float> IParameterListConvertible<float>.GetParameterList () => parameters;

            ParameterRange<float> IParameterRangeProvider<float>.GetRangeForParameterAtIndex (int index) => new ParameterRange<float>(paramRangeMin, paramRangeMax);

        }


        public class TestIndividual : NumericallyParametrizedIndividual<TestParams> {

            public override float maxMutationStrength => 0.25f;

            public override Agent CreateAgent () {
                throw new NotImplementedException();
            }

            protected override Individual GetClone () {
                return new TestIndividual() {
                    agentParams = new TestParams() {
                        parameters = (float[])this.agentParams.parameters.Clone()
                    }
                };
            }
        }

        [Test]
        public void TestProperSelfInitialization () {
            var a = new TestIndividual();
            a.InitializeWithRandomCoefficients();
            Console.WriteLine($"individual has {a.agentParams.parameters.Length} parameters");
            Assert.AreEqual(a.agentParams.parameters.Length, TestParams.DEFAULT_PARAM_COUNT);
        }

        [Test]
        public void TestProperSerializationAndDeserialization () {
            var a = new TestIndividual();
            a.InitializeWithRandomCoefficients();
            Console.WriteLine($"original has parameters {JsonSerializer.Serialize(a.agentParams.parameters)}");
            var json = JsonSerializer.Serialize(a);
            Console.WriteLine($"json is \"{json}\"");
            var clone = JsonSerializer.Deserialize<TestIndividual>(json);
            Console.WriteLine($"clone has parameters {JsonSerializer.Serialize(clone.agentParams.parameters)}");
            Assert.AreEqual(a.agentParams.parameters.Length, clone.agentParams.parameters.Length);
            for (int i = 0; i < TestParams.DEFAULT_PARAM_COUNT; i++) {
                Assert.AreEqual(a.agentParams.parameters[i], clone.agentParams.parameters[i]);
            }
        }

        const int TEST_ITERATIONS = 100;

        void TestRepeatedly (System.Action doTest) {
            for (int i = 0; i < TEST_ITERATIONS; i++) {
                doTest();
            }
        }

        TestIndividual Setup (float min, float max) {
            TestParams.paramRangeMin = min;
            TestParams.paramRangeMax = max;
            var output = new TestIndividual();
            output.InitializeWithRandomCoefficients();
            return output;
        }

        void TestRandomInitialization (float min, float max) {
            Console.WriteLine($"Testing random initialization with values from {min} to {max}");
            var a = Setup(min, max);
            TestRepeatedly(() => {
                a.InitializeWithRandomCoefficients();
                Console.WriteLine($"{JsonSerializer.Serialize(a.agentParams.parameters)}");
                Assert.AreEqual(a.agentParams.parameters.Length, TestParams.DEFAULT_PARAM_COUNT);
                foreach (var coeff in a.agentParams.parameters) {
                    if (coeff < min) {
                        Assert.Fail("Value is lower than minimum!");
                    }
                    if (coeff > max) {
                        Assert.Fail("Value is greater than maximum!");
                    }
                }
            });
        }

        [Test]
        public void TestNormalRandomInitialization () => TestRandomInitialization(-1, 1);

        [Test]
        public void TestWeirdRandomInitialization () => TestRandomInitialization(3, 7);

        void TestCombination (float min, float max) {
            Console.WriteLine("Testing combination");
            var a = new TestIndividual(){
                agentParams = new TestParams() {
                    parameters = new float[] { 0, 1, 2, 3 }
                }
            };
            Console.WriteLine($"First agent has params {JsonSerializer.Serialize(a.agentParams.parameters)}");
            var b = new TestIndividual() {
                agentParams = new TestParams() {
                    parameters = new float[] { 10, 11, 12, 13 }
                }
            };
            Console.WriteLine($"Second agent has params {JsonSerializer.Serialize(b.agentParams.parameters)}");
            TestRepeatedly(() => {
                var c = (TestIndividual)(a.CombinedClone(b));
                Console.WriteLine($"Combination has parameters {JsonSerializer.Serialize(c.agentParams.parameters)}");
                var fromA = 0;
                var fromB = 0;
                foreach (var param in c.agentParams.parameters) {
                    var isFromA = false;
                    var isFromB = false;
                    foreach (var aParam in a.agentParams.parameters) {
                        if (param == aParam) {
                            isFromA = true;
                        }
                    }
                    foreach (var bParam in b.agentParams.parameters) {
                        if (param == bParam) {
                            isFromB = true;
                        }
                    }
                    if (isFromA && isFromB) {
                        Assert.Inconclusive("Parameters must be unique for this test!");
                    }
                    if (!(isFromA || isFromB)) {
                        Assert.Inconclusive("Couldn't determine where the parameter came from!");
                    }
                    if (isFromA) fromA++;
                    if (isFromB) fromB++;
                }
                if (fromA != fromB || fromA != (TestParams.DEFAULT_PARAM_COUNT / 2)) {
                    Assert.Fail();
                }
            });
        }

        [Test]
        public void TestNormalCombination () => TestCombination(-1, 1);

        [Test]
        public void TestWeirdCombination () => TestCombination(3, 7);

        void TestInversion (float min, float max) {
            Console.WriteLine($"testing inversion of range {min} to {max}");
            var a = Setup(min, max);
            var rangePivot = (max + min) / 2;
            var approxRange = (max - min) / 10000;
            TestRepeatedly(() => {
                a.InitializeWithRandomCoefficients();
                var b = (TestIndividual)(a.InvertedClone());
                for (int i = 0; i < a.agentParams.parameters.Length; i++) {
                    var origVal = a.agentParams.parameters[i];
                    var shouldBeInverse = (-(origVal - rangePivot)) + rangePivot;
                    var newVal = b.agentParams.parameters[i];
                    Console.WriteLine($"{origVal:F4} -> {newVal:F4}");
                    Assert.LessOrEqual(Math.Abs(shouldBeInverse - newVal), approxRange);
                }
            });
        }

        [Test]
        public void TestNormalInversion () => TestInversion(-1, 1);

        [Test]
        public void TestWeirdInversion () => TestInversion(3, 7);

        void TestMutation (float min, float max) {
            Console.WriteLine($"testing mutation of range {min} to {max}");
            var a = Setup(min, max);
            TestRepeatedly(() => {
                a.InitializeWithRandomCoefficients();
                var b = (TestIndividual)(a.MutatedClone());
                for (int i = 0; i < a.agentParams.parameters.Length; i++) {
                    var origVal = a.agentParams.parameters[i];
                    var newVal = b.agentParams.parameters[i];
                    var normedDifference = (newVal - origVal) / (max - min);
                    Console.WriteLine($"{origVal:F4} -> {newVal:F4} ({(normedDifference*100):F1}%)");
                    Assert.LessOrEqual(newVal, max);
                    Assert.GreaterOrEqual(newVal, min);
                    Assert.LessOrEqual(Math.Abs(normedDifference), a.maxMutationStrength);
                }
            });
        }

        [Test]
        public void TestNormalMutation () => TestMutation(-1, 1);

        [Test]
        public void TestWeirdMutation () => TestMutation(3, 7);

    }

}
