using System.Reflection;
using NUnit.Framework;

namespace JTacticalSim.Test
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fixtures = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<TestFixtureAttribute>() != null && t.IsPublic && !t.IsAbstract)
                .OrderBy(t => t.FullName)
                .ToList();

            int passed = 0, failed = 0, skipped = 0;
            var failures = new List<string>();

            string filter = args.Length > 0 ? args[0] : null;

            foreach (var fixture in fixtures)
            {
                var tests = fixture.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttributes<TestAttribute>().Any() || m.GetCustomAttributes<TestCaseAttribute>().Any())
                    .ToList();

                foreach (var method in tests)
                {
                    var testCases = method.GetCustomAttributes<TestCaseAttribute>().ToList();
                    if (testCases.Count == 0)
                        testCases.Add(null);

                    foreach (var tc in testCases)
                    {
                        string testName = $"{fixture.Name}.{method.Name}";
                        if (tc?.Arguments?.Length > 0)
                            testName += $"({string.Join(", ", tc.Arguments)})";

                        if (filter != null && !testName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        {
                            skipped++;
                            continue;
                        }

                        if (method.GetCustomAttribute<IgnoreAttribute>() != null)
                        {
                            Console.WriteLine($"  SKIP  {testName}");
                            skipped++;
                            continue;
                        }

                        object instance = null;
                        try
                        {
                            instance = Activator.CreateInstance(fixture);
                            RunSetUp(instance, fixture);
                            var invokeArgs = BuildInvokeArgs(method, tc);
                            method.Invoke(instance, invokeArgs);
                            Console.WriteLine($"  PASS  {testName}");
                            passed++;
                        }
                        catch (TargetInvocationException tie)
                        {
                            var inner = tie.InnerException;
                            if (inner is IgnoreException)
                            {
                                Console.WriteLine($"  SKIP  {testName}");
                                skipped++;
                            }
                            else
                            {
                                Console.WriteLine($"  FAIL  {testName}");
                                failures.Add($"{testName}: {inner?.Message}");
                                failed++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  FAIL  {testName}");
                            failures.Add($"{testName}: {ex.Message}");
                            failed++;
                        }
                        finally
                        {
                            if (instance != null)
                                TryRunTearDown(instance, fixture);
                        }
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Results: {passed} passed, {failed} failed, {skipped} skipped");

            if (failures.Count > 0)
            {
                Console.WriteLine("\nFailed tests:");
                foreach (var f in failures)
                    Console.WriteLine($"  {f}");
            }

            return failed > 0 ? 1 : 0;
        }

        static object[] BuildInvokeArgs(MethodInfo method, TestCaseAttribute tc)
        {
            var rawArgs = tc?.Arguments;
            if (rawArgs == null || rawArgs.Length == 0) return null;

            var parameters = method.GetParameters();
            // Handle params array: pack all args into a single typed array
            if (parameters.Length == 1 && parameters[0].IsDefined(typeof(ParamArrayAttribute), false))
            {
                var elemType = parameters[0].ParameterType.GetElementType()!;
                var arr = Array.CreateInstance(elemType, rawArgs.Length);
                for (int i = 0; i < rawArgs.Length; i++)
                    arr.SetValue(Convert.ChangeType(rawArgs[i], elemType), i);
                return new object[] { arr };
            }
            return rawArgs;
        }

        static void RunSetUp(object instance, Type fixture)
        {
            foreach (var m in fixture.GetMethods().Where(m => m.GetCustomAttribute<SetUpAttribute>() != null))
                m.Invoke(instance, null);
        }

        static void TryRunTearDown(object instance, Type fixture)
        {
            foreach (var m in fixture.GetMethods().Where(m => m.GetCustomAttribute<TearDownAttribute>() != null))
            {
                try { m.Invoke(instance, null); } catch { }
            }
        }
    }
}
