

using System;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Aq.ExpressionJsonSerializer.Tests
{
    [TestFixture]
    public class ExpressionJsonSerializerTest
    {
        [Test]
        public void Assignment()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.A + c.B));
        }

        [Test]
        public void BitwiseAnd()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.A & c.B));
        }

        [Test]
        public void LogicalAnd()
        {
            TestExpression((Expression<Func<Context, bool>>) (c => c.A > 0 && c.B > 0));
        }

        [Test]
        public void ArrayIndex()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.Array[0]));
        }

        [Test]
        public void ArrayLength()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.Array.Length));
        }

        [Test]
        public void Method()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.Method()));
        }

        [Test]
        public void MethodWithArguments()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.Method("B")));
        }

        [Test]
        public void Coalesce()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.C ?? c.A));
        }

        [Test]
        public void Conditional()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.C == null ? c.A : c.B));
        }

        [Test]
        public void Convert()
        {
            TestExpression((Expression<Func<Context, int>>) (c => (short) (c.C ?? 0)));
        }

        [Test]
        public void Decrement()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.A - 1));
        }

        [Test]
        public void DivisionWithCast()
        {
            TestExpression((Expression<Func<Context, float>>) (c => (float) c.A / c.B));
        }

        [Test]
        public void Equality()
        {
            TestExpression((Expression<Func<Context, bool>>) (c => c.A == c.B));
        }

        [Test]
        public void Xor()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.A ^ c.B));
        }

        [Test]
        public void LinqExtensions()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.Array.FirstOrDefault()));
        }

        [Test]
        public void GreaterThan()
        {
            TestExpression((Expression<Func<Context, bool>>) (c => c.A > c.B));
        }

        [Test]
        public void Increment()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.A + 1));
        }

        [Test]
        public void Indexer()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c["A"]));
        }

        [Test]
        public void Invoke()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.Func()));
        }

        [Test]
        public void Constant()
        {
            TestExpression((Expression<Func<Context, bool>>) (c => false));
        }

        [Test]
        public void Lambda()
        {
            TestExpression((Expression<Func<Context, int>>) (c => ((Func<Context, int>) (_ => _.A))(c)));
        }

        [Test]
        public void LeftShift()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.A << c.C ?? 0));
        }

        [Test]
        public void PropertyAccess()
        {
            TestExpression((Expression<Func<Context, int>>) (c => c.B));
        }

        [Test]
        public void Negation()
        {
            TestExpression((Expression<Func<Context, int>>) (c => -c.A));
        }

        [Test]
        public void New()
        {
            TestExpression((Expression<Func<Context, object>>) (c => new object()));    
        }

        [Test]
        public void NewWithArguments()
        {
            TestExpression((Expression<Func<Context, object>>) (c => new String('s', 1)));
        }

        [Test]
        public void InitArray()
        {
            TestExpression((Expression<Func<Context, int[]>>) (c => new[] { 0 }));
        }

        [Test]
        public void InitEmptyArray()
        {
            TestExpression((Expression<Func<Context, int[,]>>) (c => new int[3, 2]));
        }

        [Test]
        public void TypeAs()
        {
            TestExpression((Expression<Func<Context, object>>) (c => c as object));
        }

        [Test]
        public void TypeOf()
        {
            TestExpression((Expression<Func<Context, bool>>) (c => typeof(Context) == c.GetType()));
        }

        [Test]
        public void TypeIs()
        {
            TestExpression((Expression<Func<Context, bool>>) (c => c is object));
        }

        [Test]
        public void MethodResultCast()
        {
            TestExpression((Expression<Func<Context, int>>) (c => (int) c.Method3()));
        }

        private sealed class Context
        {
            public int A;
            public int B { get; set; }
            public int? C;
            public int[] Array;
            public int this[string key]
            {
                get
                {
                    switch (key) {
                        case "A": return A;
                        case "B": return B;
                        case "C": return C ?? 0;
                        default: throw new NotImplementedException();
                    }
                }
            }
            public Func<int> Func;
            public int Method() { return A; }
            public int Method(string key) { return this[key]; }
            public object Method3() { return A; }
        }

        private static void TestExpression(LambdaExpression source)
        {
            var random = new Random();
            int u;
            var context = new Context {
                A = random.Next(),
                B = random.Next(),
                C = (u = random.Next(0, 2)) == 0 ? null : (int?) u,
                Array = new[] { random.Next() },
                Func = () => u
            };

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ExpressionJsonConverter());

            var json = JsonConvert.SerializeObject(source, settings);
            var target = JsonConvert.DeserializeObject<LambdaExpression>(json, settings);

            Assert.AreEqual(
                ExpressionResult(source, context),
                ExpressionResult(target, context)
            );
        }

        private static string ExpressionResult(LambdaExpression expr, Context context)
        {
            return JsonConvert.SerializeObject(expr.Compile().DynamicInvoke(context));
        }
    }
}
