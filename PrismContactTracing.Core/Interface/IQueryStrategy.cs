using System.Data;

namespace PrismContactTracing.Core.Interface {
    public interface IQueryStrategy {
        public DataTable DoQuery();
    }
}
