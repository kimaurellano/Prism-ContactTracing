using PrismContactTracing.Core.Interface;
using System.Data;

namespace PrismContactTracing.Core.DataComponent {
    public class QueryStrategy {

        private IQueryStrategy _queryStrategy;

        public DataTable MainDataTable { get; set; }

        public void SetQuery(IQueryStrategy queryStrategy) {
            _queryStrategy = queryStrategy;
            MainDataTable = _queryStrategy.DoQuery();
        }
    }
}
