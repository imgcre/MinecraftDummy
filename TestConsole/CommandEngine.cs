using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    partial class TestClient
    {
        private class CommandAttribute : Attribute { }

        private class CommandInnerException : Exception
        {
            public CommandInnerException(string message) : base(message)
            {

            }
        }


        private object InvokeCommand(string commandName, params string[] parameters)
        {
            //如果存在，再判断参数是否匹配
            //Invalid overload
            
            var command = Array.Find(typeof(TestClient).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase)
                    , c => c.IsDefined(typeof(CommandAttribute)) && c.GetParameters().Length.Equals(parameters.Count()) && c.Name.ToUpper() == commandName.ToUpper());

            if (commandName.ToUpper() == nameof(Invoke).ToUpper())
            {
                command = typeof(TestClient).GetMethod(commandName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            }

            return InvokeCommandInternal(this, command, parameters);
        }


        private object InvokeCommandInternal(object obj, MethodInfo command, params string[] parameters)
        {
            if (command != null)
            {
                var paramList = new List<object>();
                foreach (var formParam in command.GetParameters())
                {
                    var paramType = formParam.ParameterType;
                    var currentParam = default(string);

                    //invoke为空
                    try
                    {
                        currentParam = parameters[formParam.Position];
                    }
                    catch(IndexOutOfRangeException)
                    {
                        currentParam = string.Empty;
                    }

                    

                    if (paramType.Equals(typeof(string)))
                        paramList.Add(currentParam);
                    else if (paramType.Equals(typeof(string[])))
                        //跳过形参前的实参阵元
                        paramList.Add(parameters.Skip(formParam.Position).ToArray());
                    else
                    {
                        var TryParse = Array.Find(paramType.GetMethods(BindingFlags.Public | BindingFlags.Static), (t) => t.Name == "TryParse");
                        if (TryParse == null)
                            throw new CommandDamagedException($"bad command: {command.Name}'s argument's type {paramType.Name} is invalid");
                        else
                        {
                            var array = new object[] { currentParam, Activator.CreateInstance(paramType) };
                            if (!(bool)TryParse.Invoke(null, array))
                                throw new ArgumentException($"invoke fault: {command.Name}'s argument {currentParam} is wrong");
                            paramList.Add(array.Last());
                        }
                    }
                }
                Console.WriteLine($"将执行的命令:{command.Name}");
                return command.Invoke(obj, paramList.ToArray());


            }
            else throw new CommandNotFoundException($"unknown command");
        }


        private class CommandNotFoundException : ArgumentException
        {
            public CommandNotFoundException(string message) : base(message) { }
        }

        private class CommandDamagedException : ArgumentException
        {
            public CommandDamagedException(string message) : base(message) { }
        }
    }
}
