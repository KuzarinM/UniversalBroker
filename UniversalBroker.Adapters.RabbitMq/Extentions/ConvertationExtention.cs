using AutoMapper.Internal;
using Google.Protobuf.Collections;
using Newtonsoft.Json;
using Protos;
using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace UniversalBroker.Adapters.RabbitMq.Extentions
{
    public static class ConvertationExtention
    {
        public static T GetModelFromAttributes<T>(this RepeatedField<AttributeDto> attributes)
        {
            var model = (T)Activator.CreateInstance(typeof(T))!;

            model.SetValueFromAttributes(attributes);

            return model;
        }

        public static int SetValueFromAttributes<T>(this T model, RepeatedField<AttributeDto> attributes) 
        {
            var type = typeof(T);

            int updatedFields = 0;

            foreach (var property in type.GetProperties()) 
            {
                try
                {
                    if (property.CanWrite)
                    {
                        var attribute = attributes.FirstOrDefault(x => x.Name == $"{type.Name}.{property.Name}");

                        if (attribute == null)
                        {
                            attribute = attributes.FirstOrDefault(x => x.Name == property.Name);
                        }

                        if(attribute != null)
                        {
                            TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
                            property.SetValue(model, converter.ConvertFromString(attribute.Value));

                            updatedFields++;
                        }
                    }
                }
                catch (Exception ex) 
                {

                }
            }

            foreach (var field in type.GetFields())
            {
                try
                {
                    if (field.CanBeSet())
                    {
                        var attribute = attributes.FirstOrDefault(x => x.Name == $"{type.Name}.{field.Name}");

                        if (attribute == null)
                        {
                            attribute = attributes.FirstOrDefault(x => x.Name == field.Name);
                        }

                        if (attribute != null)
                        {
                            TypeConverter converter = TypeDescriptor.GetConverter(field.FieldType);
                            field.SetValue(model, converter.ConvertFromString(attribute.Value));

                            updatedFields++;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return updatedFields;
        }

        public static int GetAttributesFromModel<T>(this T model, RepeatedField<AttributeDto> existing)
        {
            var defaultModel = (T)Activator.CreateInstance(typeof(T))!;

            int updatedCount = 0;

            var type = typeof(T);

            foreach (var property in type.GetProperties())
            {
                try
                {
                    if (property.CanRead)
                    {
                        var propertyType = property.PropertyType;
                        
                        if(propertyType.IsValueType || 
                            propertyType.IsPrimitive || 
                            propertyType == typeof(string) || 
                            propertyType == typeof(DateTime) || 
                            propertyType == typeof(TimeSpan) || 
                            propertyType.IsEnum)
                        {
                            var value = property.GetValue(model, null);
                            var defaultValue = property.GetValue(defaultModel, null);

                            if (value == null && defaultValue == null)
                                continue;

                            if(defaultValue == null || value == null || !defaultValue.Equals(value))
                            {
                                var name= $"{type.Name}.{property.Name}";

                                var shortAttribute = existing.FirstOrDefault(a => property.Name == a.Name);

                                if (shortAttribute != null)
                                    existing.Remove(shortAttribute);

                                var valueStr = value?.ToString() ?? "null";

                                updatedCount += AddOrUpdateAttribute(existing, name, valueStr);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            foreach (var property in type.GetFields())
            {
                try
                {

                    var propertyType = property.FieldType;

                    if (propertyType.IsValueType ||
                        propertyType.IsPrimitive ||
                        propertyType == typeof(string) ||
                        propertyType == typeof(DateTime) ||
                        propertyType == typeof(TimeSpan) ||
                        propertyType.IsEnum)
                    {
                        var value = property.GetValue(model);
                        var defaultValue = property.GetValue(defaultModel);

                        if (value == null && defaultValue == null)
                            continue;

                        if (defaultValue == null || value == null || !defaultValue.Equals(value))
                        {
                            var name = $"{type.Name}.{property.Name}";

                            var shortAttribute = existing.FirstOrDefault(a => property.Name == a.Name);

                            if (shortAttribute != null)
                                existing.Remove(shortAttribute);

                            var valueStr = JsonConvert.SerializeObject(value);

                            updatedCount += AddOrUpdateAttribute(existing, name, valueStr);
                        }
                    }   
                }
                catch (Exception ex)
                {

                }
            }

            return updatedCount;
        }

        public static int AddOrUpdateAttribute(this RepeatedField<AttributeDto> attributes, string name, string value)
        {
            var existAttribute = attributes.FirstOrDefault(a => a.Name == name);

            if (existAttribute != null && existAttribute.Value != value)
            {
                existAttribute.Value = value;
                return 1;
            }
            else if(existAttribute == null)
            {
                attributes.Add(new AttributeDto()
                {
                    Name = name,
                    Value = value
                });

                return 1;
            }
            return 0;
        }
    }
}
