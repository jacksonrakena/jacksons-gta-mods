using System;

namespace ModCommon
{
    public class ModConfigSection
    {
        private IModConfigProperty<string> DefineString(string name, string @default)
        {
            return new StringModConfigProperty(this, name, @default);
        }

        private IModConfigProperty<T> DefineEnum<T>(string name, T @default) where T : Enum
        {
            return new EnumModConfigProperty<T>(this, name, @default);
        }
        private readonly string _sectionTitle;
        private ModConfig _instance;
        public ModConfigSection(ModConfig instance, string sectionTitle)
        {
            _instance = instance;
            _sectionTitle = sectionTitle;
        }

        public T GetEnum<T>(string name, T @default) where T : struct, Enum
        {
            var value = _instance.ReadValue(_sectionTitle, name);
            if (value == null) return @default;
            return Enum.TryParse<T>(value, out var res) ? res : @default;
        }

        public bool GetBoolean(string name, bool def)
        {
            var value = _instance.ReadValue(_sectionTitle, name);
            if (value == null || !bool.TryParse(value, out var valueBool)) return def;
            return valueBool;
        }

        public string? Get(string name)
        {
            return _instance.ReadValue(_sectionTitle, name);
        }

        public bool Set(string name, string value)
        {
            return _instance.WriteValue(_sectionTitle, name, value);
        }
    }
    public interface IModConfigProperty<T>
    {
        public T GetDefault();
        public T Get();
        public bool Set(T value);
    }

    public class StringModConfigProperty : IModConfigProperty<string>
    {
        private string _name;
        private ModConfigSection _section;
        private string _default;
        public StringModConfigProperty(ModConfigSection section, string name, string @default)
        {
            _section = section;
            _default = @default;
            _name = name;
        }

        public string GetDefault()
        {
            return _default;
        }
        public string Get()
        {
            return _section.Get(_name) ?? _default;
        }

        public bool Set(string value)
        {
            return _section.Set(_name, value);
        }
    }

    public class EnumModConfigProperty<T> : IModConfigProperty<T> where T : Enum
    {
        private StringModConfigProperty _backingProperty;

        public EnumModConfigProperty(ModConfigSection section, string name, T @default)
        {
            _backingProperty = new StringModConfigProperty(section, name, Enum.GetName(@default.GetType(), @default)!);
        }
        public T GetDefault()
        {
            return (T)Enum.Parse(typeof(T), _backingProperty.GetDefault());
        }

        public T Get()
        {
            return (T)Enum.Parse(typeof(T), _backingProperty.Get());
        }

        public bool Set(T value)
        {
            return _backingProperty.Set(Enum.GetName(typeof(T), value)!);
        }
    }
}