using System.Data;
using Dapper;

namespace PeminjamanRuangAPI.Data.TypeHandlers
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.Value = value;
        }

        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateOnly dateOnly => dateOnly,
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                _ => throw new DataException(
                    $"Tidak dapat mengubah {value.GetType()} menjadi DateOnly.")
            };
        }
    }
}