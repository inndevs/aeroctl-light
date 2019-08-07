using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace AeroCtl.UI
{
	public class EnumerationExtension : MarkupExtension
	{
		private Type enumType;

		public Type EnumType
		{
			get => this.enumType;
			private set
			{
				if (this.enumType == value)
					return;

				var enumType = Nullable.GetUnderlyingType(value) ?? value;

				if (enumType.IsEnum == false)
					throw new ArgumentException("Type must be an Enum.");

				this.enumType = value;
			}
		}

		public EnumerationExtension(Type enumType)
		{
			this.EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var enumValues = Enum.GetValues(EnumType);

			return (
				from object enumValue in enumValues
				select new EnumerationMember{
					Value = enumValue,
					Description = GetDescription(enumValue)
				}).ToArray();
		}

		private string GetDescription(object enumValue)
		{
			return this.EnumType
				.GetField(enumValue.ToString())
				.GetCustomAttributes(typeof(DescriptionAttribute), false)
				.FirstOrDefault() is DescriptionAttribute descriptionAttribute
				? descriptionAttribute.Description
				: enumValue.ToString();
		}

		public class EnumerationMember
		{
			public string Description { get; set; }
			public object Value { get; set; }
		}
	}
}
