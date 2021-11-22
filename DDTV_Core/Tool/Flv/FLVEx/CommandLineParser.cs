using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DDTV_Core.Tool.Flv.FLVEx
{
#if SHARED_PUBLIC_API
  public
#else
    internal
#endif
  class CommandLineParser<TModel>
      where TModel : class
    {
        public const string ARG_PREFIX1 = "-";
        public const string ARG_PREFIX2 = "/";

        private readonly string[] HELP_COMMANDS = new string[]
        {
      ARG_PREFIX1 + "?",
      ARG_PREFIX2 + "?",
      "help"
        };

        private readonly Dictionary<Type, Func<Type, string, object>> parsers = new Dictionary<Type, Func<Type, string, object>>
    {
      { typeof(bool), (t, value) => bool.Parse(value) },
      { typeof(int), (t, value) => int.Parse(value) },
      { typeof(double), (t, value) => double.Parse(value) },
      { typeof(byte), (t, value) => byte.Parse(value) },
      { typeof(string), (t, value) => value },
      { typeof(float), (t, value) => float.Parse(value) },
      { typeof(uint), (t, value) => uint.Parse(value) },
    };

        private List<CommandDescriptor> descs = new List<CommandDescriptor>();
        private List<CommandDescriptor> unnamedDescs = new List<CommandDescriptor>();
        private List<CommandDescriptor> allDescs = new List<CommandDescriptor>();
        private CommandModelAttribute modelAttribute;
        private TModel model;
        private StringComparer stringComparer = StringComparer.InvariantCultureIgnoreCase;
        private bool suppressUnknownArgs;
        private bool assignmentSyntax;
        private bool writeUsageOnError;
        private string error;

        private class CommandDescriptor
        {
            public readonly CommandAttribute Attribute;

            protected bool isSet;
            protected readonly TModel model;
            protected readonly PropertyInfo Property;

            protected CommandDescriptor(CommandAttribute attribute, PropertyInfo property, CommandLineParser<TModel> parser)
            {
                Attribute = attribute;
                Property = property;
                model = parser.model;
            }

            public virtual void SetFlagValue(bool value)
            {
                throw new NotSupportedException();
            }

            public virtual string SetValue(string value)
            {
                throw new NotSupportedException();
            }

            public bool IsRequiredSet
            {
                get { return isSet || !Attribute.IsRequired; }
            }
        }

        private class PropertyDescriptor : CommandDescriptor
        {
            private readonly Func<Type, string, object> valueParser;

            public PropertyDescriptor(CommandAttribute attribute, PropertyInfo property, CommandLineParser<TModel> parser)
              : base(attribute, property, parser)
            {
                if (property.PropertyType.IsEnum)
                    valueParser = Enum.Parse;
                else if (!parser.parsers.TryGetValue(property.PropertyType, out valueParser))
                    throw new InvalidOperationException($"Not supported property type: {property.PropertyType}");
            }

            public override string SetValue(string value)
            {
                try
                {
                    Property.SetValue(model, valueParser(Property.PropertyType, value));
                    isSet = true;
                }
                catch (Exception e)
                {
                    return e.Message;
                }

                return null;
            }
        }

        private class NullablePropertyDescriptor : CommandDescriptor
        {
            private readonly Func<Type, string, object> valueParser;
            private readonly Type type;

            public NullablePropertyDescriptor(CommandAttribute attribute, PropertyInfo property, CommandLineParser<TModel> parser)
              : base(attribute, property, parser)
            {
                type = property.PropertyType.GenericTypeArguments[0];
                if (type.IsEnum)
                    valueParser = Enum.Parse;
                else if (!parser.parsers.TryGetValue(type, out valueParser))
                    throw new InvalidOperationException($"Not supported property type: {type}");
            }

            public override string SetValue(string value)
            {
                try
                {
                    Property.SetValue(model, valueParser(type, value));
                    isSet = true;
                }
                catch (Exception e)
                {
                    return e.Message;
                }

                return null;
            }
        }

        private class ListDescriptor : CommandDescriptor
        {
            private readonly object listObject;
            private readonly MethodInfo listAddMethod;
            private readonly Func<Type, string, object> valueParser;

            public ListDescriptor(CommandAttribute attribute, PropertyInfo property, CommandLineParser<TModel> parser)
              : base(attribute, property, parser)
            {
                Type type = property.PropertyType.GetGenericArguments()[0];

                // get list itself
                listObject = property.GetValue(model);
                if (listObject == null)
                    throw new ArgumentNullException(null, $"{property.Name} should be initialized.");
                listAddMethod = property.PropertyType.GetMethod("Add");

                // get parser
                if (type.IsEnum)
                    valueParser = Enum.Parse;
                else if (!parser.parsers.TryGetValue(type, out valueParser))
                    throw new InvalidOperationException($"Not supported property type: {type}");
            }

            public override string SetValue(string value)
            {
                try
                {
                    if (listObject != null)
                        listAddMethod.Invoke(listObject, new[] { valueParser(Property.PropertyType, value) });

                    isSet = true;
                }
                catch (Exception e)
                {
                    return e.Message;
                }

                return null;
            }
        }

        private class FlagDescriptor : CommandDescriptor
        {
            public FlagDescriptor(CommandAttribute attribute, PropertyInfo property, CommandLineParser<TModel> parser) : base(attribute, property, parser) { }

            public override void SetFlagValue(bool value)
            {
                Property.SetValue(model, value);
            }
        }

        /// <summary>
        /// Creates the commandline parser instance and analyzes the model type.
        /// </summary>
        /// <param name="model">Model object to fill it with values from commandline args.</param>
        /// <exception cref="ArgumentException">Thrown if the model has flag property with non-boolean type or the model has a property without setter.</exception>
        /// <exception cref="InvalidOperationException">Thrown if model has a property with unsupported type.</exception>
        public CommandLineParser(TModel model)
        {
            this.model = model;

            Type modelType = typeof(TModel);
            modelAttribute = modelType.GetCustomAttribute<CommandModelAttribute>();
            Dictionary<int, CommandDescriptor> unnamedDict = new Dictionary<int, CommandDescriptor>();

            PropertyInfo[] infos = modelType.GetProperties();
            foreach (PropertyInfo info in infos)
            {
                CommandAttribute attr = info.GetCustomAttribute<CommandAttribute>();
                if (attr == null)
                    continue;

                if (attr.IsFlag && info.PropertyType != typeof(bool))
                    throw new ArgumentException($"{info.Name} is marked as flag but has type {info.PropertyType.Name}. Only boolean property can be flag.");

                if (!info.CanWrite)
                    throw new ArgumentException($"{info.Name} has no setter.");

                CommandDescriptor desc;

                if (attr.IsFlag)
                    desc = new FlagDescriptor(attr, info, this);
                else
                {
                    if (info.PropertyType.IsGenericType)
                    {
                        if (info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            desc = new NullablePropertyDescriptor(attr, info, this);
                        }
                        else
                        {
                            Type[] ifaces = info.PropertyType.GetInterfaces();

                            bool iCollectionFound = false;
                            foreach (Type iface in ifaces)
                            {
                                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ICollection<>))
                                {
                                    iCollectionFound = true;
                                    break;
                                }
                            }

                            if (!iCollectionFound)
                                throw new ArgumentException($"Invalid field {info.Name}. Only ICollection<> implementations are allowed for generics.");

                            desc = new ListDescriptor(attr, info, this);
                        }
                    }
                    else
                        desc = new PropertyDescriptor(attr, info, this);
                }

                if (attr.Name == null)
                    unnamedDict.Add(desc.Attribute.UnnamedIndex, desc);
                else
                    descs.Add(desc);

                allDescs.Add(desc);
            }

            int lastRequiredIndex = -1;
            for (int i = 0; i < unnamedDict.Count; i++)
            {
                CommandDescriptor desc;
                if (!unnamedDict.TryGetValue(i, out desc))
                    throw new ArgumentException($"Missing unnamed index {i}");

                if (desc.Attribute.IsRequired)
                {
                    if (i > lastRequiredIndex + 1)
                        throw new ArgumentException($"Can't set unnamed arg #{i} as required as unnamed args before are not required.");

                    lastRequiredIndex = i;
                }

                unnamedDescs.Add(desc);
            }
        }

        private void OnError(string error)
        {
            this.error = error;
            Console.WriteLine(error);
            if (writeUsageOnError)
                WriteUsage();
        }

        /// <summary>
        /// Parse the command line and fill model with values.
        /// </summary>
        /// <param name="args">Commandline args from environment.</param>
        /// <returns>True if parsing successfull and application should continue. False to exit immediately (on errors or help request).</returns>
        public bool Parse(string[] args)
        {
            // welcome
            if (modelAttribute != null)
            {
                Console.WriteLine(modelAttribute.WelcomeText);
                Console.WriteLine();
            }

            // help?
            if (args.Length > 0)
                for (int i = 0; i < HELP_COMMANDS.Length; i++)
                    if (stringComparer.Equals(args[0], HELP_COMMANDS[i]))
                    {
                        WriteUsage();
                        return false;
                    }

            CommandDescriptor desc = null;
            int unnamedIndex = 0;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith(ARG_PREFIX1) || arg.StartsWith(ARG_PREFIX2))
                {
                    if (desc != null)
                    {
                        OnError(string.Format("Missing value for argument: {0}", desc.Attribute.Name));
                        return false;
                    }

                    if (assignmentSyntax)
                    {
                        int index = arg.IndexOf('=');
                        if (index != -1)
                        {
                            desc = FindDesc(arg.Substring(1, index - 1));
                            if (desc.Attribute.IsFlag)
                            {
                                OnError(string.Format("Unable to assign flag: {0}", desc.Attribute.Name));
                                return false;
                            }

                            desc.SetValue(arg.Substring(index + 1));
                            desc = null;
                            continue;
                        }
                    }

                    desc = FindDesc(arg.Substring(1));
                    if (desc == null)
                    {
                        if (suppressUnknownArgs)
                            continue;

                        OnError(string.Format("Invalid argument: {0}", arg));
                        return false;
                    }

                    if (desc.Attribute.IsFlag)
                    {
                        desc.SetFlagValue(true);
                        desc = null;
                    }
                }
                else
                {
                    if (desc == null)
                    {
                        if (unnamedIndex >= unnamedDescs.Count)
                        {
                            if (suppressUnknownArgs)
                                continue;

                            OnError(string.Format("Invalid unnamed argument: {0}", arg));
                            return false;
                        }

                        unnamedDescs[unnamedIndex++].SetValue(arg);
                    }
                    else
                    {
                        string error = desc.SetValue(arg);
                        if (error != null)
                        {
                            OnError(string.Format("Failed to parse {0}: {1}", desc.Attribute.Name, error));
                            return false;
                        }
                        desc = null;
                    }
                }
            }

            if (desc != null)
            {
                OnError(string.Format("Missing value for argument: {0}", desc.Attribute.Name));
                return false;
            }

            // validate
            for (int i = 0; i < allDescs.Count; i++)
            {
                CommandDescriptor d = allDescs[i];

                if (!d.IsRequiredSet)
                {
                    if (d.Attribute.Name != null)
                        OnError(string.Format("Required argument is missing: {0}", d.Attribute.Name));
                    else
                        OnError(string.Format("Required argument is missing: #{0}", d.Attribute.UnnamedIndex));
                    return false;
                }
            }

            return true;
        }

        private CommandDescriptor FindDesc(string value)
        {
            for (int i = 0; i < descs.Count; i++)
            {
                CommandDescriptor desc = descs[i];
                if (stringComparer.Equals(desc.Attribute.Name, value))
                    return desc;

                if (!string.IsNullOrEmpty(desc.Attribute.Alias) && stringComparer.Equals(desc.Attribute.Alias, value))
                    return desc;
            }

            return null;
        }

        private void WriteUsage()
        {
            if (modelAttribute != null)
            {
                if (!string.IsNullOrWhiteSpace(modelAttribute.UsageText))
                {
                    Console.WriteLine(modelAttribute.UsageText);
                    Console.WriteLine();
                }
            }

            Console.WriteLine("Usage:");
            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
                Console.Write("app.exe");
            else
                Console.Write(Path.GetFileName(assembly.Location));

            foreach (CommandDescriptor desc in allDescs)
            {
                if (desc.Attribute.Name == null)
                {
                    if (desc.Attribute.IsRequired)
                        Console.Write(" {0}", desc.Attribute.UsageExample);
                    else
                        Console.Write(" [{0}]", desc.Attribute.UsageExample);
                }
                else
                {
                    if (desc.Attribute.IsFlag)
                        Console.Write(" [{0}{1}]", ARG_PREFIX1, desc.Attribute.Name);
                    else if (desc.Attribute.IsRequired)
                        Console.Write(" {0}{1} {2}", ARG_PREFIX1, desc.Attribute.Name, desc.Attribute.UsageExample);
                    else
                        Console.Write(" [{0}{1} {2}]", ARG_PREFIX1, desc.Attribute.Name, desc.Attribute.UsageExample);
                }
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Arguments:");

            foreach (CommandDescriptor desc in allDescs)
            {
                if (string.IsNullOrEmpty(desc.Attribute.Usage))
                    continue;

                string name;
                if (desc.Attribute.Name == null)
                    name = desc.Attribute.UsageExample;
                else
                {
                    name = ARG_PREFIX1 + desc.Attribute.Name;
                    if (!string.IsNullOrEmpty(desc.Attribute.Alias))
                        name += ", " + ARG_PREFIX1 + desc.Attribute.Alias;
                }

                string comment = desc.Attribute.Usage;
                if (!desc.Attribute.IsRequired)
                    comment += " Optional.";

                const int ARG_WIDTH = 16;

                name = name.PadRight(ARG_WIDTH);
                if (name.Length > ARG_WIDTH)
                    name += " ";
                Console.Write(name);
                int i = 0;

                int totalLength = int.MaxValue;
                if (!Console.IsOutputRedirected)
                    totalLength = Console.WindowWidth - name.Length;

                while (comment.Length - i > totalLength)
                {
                    Console.Write(comment.Substring(i, totalLength).PadLeft(ARG_WIDTH));
                    Console.Write("".PadRight(ARG_WIDTH));
                    i += totalLength;
                    totalLength = Console.WindowWidth - ARG_WIDTH;
                }

                if (comment.Length - i == totalLength)
                    Console.Write(comment.Substring(i));
                else
                    Console.WriteLine(comment.Substring(i));
            }
        }

        /// <summary>
        /// Gets the parser's model.
        /// </summary>
        public TModel Model
        {
            get { return model; }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the parser should be case-sensitive (used for arg names).
        /// </summary>
        public bool CaseSensitive
        {
            get { return stringComparer != StringComparer.InvariantCultureIgnoreCase; }
            set
            {
                if (value)
                    stringComparer = StringComparer.InvariantCulture;
                else
                    stringComparer = StringComparer.InvariantCultureIgnoreCase;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the parser should ignore unknown arguments. Will fail if disabled.
        /// </summary>
        public bool SuppressUnknownArgs
        {
            get { return suppressUnknownArgs; }
            set { suppressUnknownArgs = value; }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the assignment syntax is enabled. Assignment syntax allows to set args as
        /// <code>/arg=value</code>
        /// along with usual
        /// <code>/arg value</code>
        /// </summary>
        /// <remarks>If this syntax is enabled, the values cannot have '=' symbol.</remarks>
        public bool AssignmentSyntax
        {
            get { return assignmentSyntax; }
            set { assignmentSyntax = value; }
        }

        /// <summary>
        /// Gets or sets the value indicating whether usages should written on error.
        /// </summary>
        public bool WriteUsageOnError
        {
            get { return writeUsageOnError; }
            set { writeUsageOnError = value; }
        }

        /// <summary>
        /// Gets the error if <see cref="Parse"/> returned false.
        /// </summary>
        public string Error
        {
            get { return error; }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
#if SHARED_PUBLIC_API
  public
#else
    internal
#endif
  class CommandAttribute : Attribute
    {
        public int UnnamedIndex { get; }
        public string Usage { get; }
        public string UsageExample { get; }
        public string Name { get; }
        public string Alias { get; }
        public bool IsRequired { get; }
        public bool IsFlag { get; }

        /// <summary>
        /// Creates the attribute instance for named command.
        /// </summary>
        /// <param name="usage">Usage description for the parameter.</param>
        /// <param name="name">Parameter name to use in commandline.</param>
        /// <param name="alias">Parameter alias (alternative name) to use in commandline.</param>
        /// <param name="usageExample">Value example in usage.</param>
        /// <param name="isRequired">True if the param is required and false otherwise.</param>
        /// <param name="isFlag">Is flag, no value required. Valid only for boolean properties.</param>
        public CommandAttribute(string name, string usage, string usageExample = "value", string alias = null, bool isRequired = false, bool isFlag = false)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (isRequired && isFlag)
                throw new ArgumentException("Invalid settings. Property can't be flag and required at same time.");

            Usage = usage;
            Name = name;
            Alias = alias;
            IsRequired = isRequired;
            IsFlag = isFlag;
            UsageExample = usageExample;
            UnnamedIndex = -1;
        }

        /// <summary>
        /// Creates the attribute instance for unnamed command.
        /// </summary>
        /// <param name="unnamedIndex">Zero-based index of unnamed argument.</param>
        /// <param name="usage">Usage description for the parameter.</param>
        /// <param name="alias">Parameter alias (alternative name) to use in commandline.</param>
        /// <param name="usageExample">Value example in usage.</param>
        /// <param name="isRequired">True if the param is required and false otherwise.</param>
        public CommandAttribute(int unnamedIndex, string usage, string usageExample = "value", string alias = null, bool isRequired = false)
        {
            UnnamedIndex = unnamedIndex;
            Usage = usage;
            UsageExample = usageExample;
            Alias = alias;
            IsRequired = isRequired;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
#if SHARED_PUBLIC_API
  public
#else
    internal
#endif
  class CommandModelAttribute : Attribute
    {
        public string WelcomeText { get; }
        public string UsageText { get; }

        /// <summary>
        /// Creates the attribute instance.
        /// </summary>
        /// <param name="welcomeText">Welcome text for commandline interface: copyright, etc.</param>
        /// <param name="usageText">Additional usage text.</param>
        public CommandModelAttribute(string welcomeText, string usageText = null)
        {
            WelcomeText = welcomeText;
            UsageText = usageText;
        }
    }
}