using System.Data;
using Dapper;

namespace PeminjamanRuangAPI.Data.TypeHandlers
{
    public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.Value = value;
        }

        public override TimeOnly Parse(object value)
        {
            return value switch
            {
                TimeOnly timeOnly => timeOnly,
                TimeSpan timeSpan => TimeOnly.FromTimeSpan(timeSpan),
                _ => throw new DataException(
                    $"Tidak dapat mengubah {value.GetType()} menjadi TimeOnly.")
            };
        }
    }
}