<Query Kind="Program" />

void Main()
{
}
	public abstract class BusinessObject<T> where T : BusinessObject<T>
	    {
	        #region Constructor
	
	        /// <summary>
	        /// Initializes a new instance of the <see cref="BusinessObject"/> class.
	        /// </summary>
	        protected BusinessObject(string connectionString)
	        {
	            ConnectionString = connectionString;
	        }
	
	        #endregion Constructor
	
	   		#region Properties
	
	        /// <summary>
	        /// Gets the connection string.  
	        /// </summary>
	        /// <value>The connection string.</value>
	        public virtual string ConnectionString { get; protected set; }
	
	        #endregion Properties
	
	
	        #region Methods
	
	        protected virtual void UseConnection(Action withConnectionAction, SqlConnection conn)
	        {
	            if (conn.State == ConnectionState.Closed)
	                conn.Open();
	            withConnectionAction();
	        }
	
	        protected TResult Get<TResult>(Func<SqlConnection, TResult> withConnectionFunc, string connectionString = null)
	        {
	            using (SqlConnection conn = new SqlConnection(connectionString ?? ConnectionString))
	            {
	                conn.Open();
	                return withConnectionFunc(conn);
	            }
	        }
	
	        /// <summary>
	        /// Before this method: return Get(conn => GetAnswerByAnswerGuid(answerGuid, conn, null));
	        /// Using this method: return Get(answerGuid, GetAnswerByAnswerGuid);
	        /// </summary>
	        protected TResult Get<TInput, TResult>(TInput input, Func<TInput, SqlConnection, SqlTransaction, TResult> withConnectionFunc)
	        {
	            return Get(conn => withConnectionFunc(input, conn, null));
	        }
	
	        /// <summary>
	        /// Before this method: return Get(conn => GetByProjectQuotaIdAndDemographicId(projectQuotaId,demographicId, conn, null));
	        /// Using this method: return Get(projectQuotaId, demographicId, GetByProjectQuotaIdAndDemographicId);
	        /// </summary>
	        protected TResult Get<TInput1, TInput2, TResult>(TInput1 input1, TInput2 input2, Func<TInput1, TInput2, SqlConnection, SqlTransaction, TResult> withConnectionFunc)
	        {
	            return Get(conn => withConnectionFunc(input1, input2, conn, null));
	        }
	
	        protected void Execute(Action<SqlConnection> withConnectionAction, string connectionString = null)
	        {
	            using (SqlConnection conn = new SqlConnection(connectionString ?? ConnectionString))
	            {
	                conn.Open();
	                withConnectionAction(conn);
	            }
	        }
	
	        protected virtual void ExecWithTransaction(Action<SqlConnection, SqlTransaction> transactionalAction)
	        {
	            Execute(conn =>
	            {
	                using (SqlTransaction trans = conn.BeginTransaction())
	                {
	                    transactionalAction(conn, trans);
	                    trans.Commit();
	                }
	            });
	        }
	
	        protected void ExecWithTransaction<TInput>(TInput input, Action<TInput, SqlConnection, SqlTransaction> transactionalAction)
	        {
	            ExecWithTransaction((conn, trans) => transactionalAction(input, conn, trans));
	        }
	
	        protected void ExecWithTransaction<TInput1, TInput2>(TInput1 input1, TInput2 input2, Action<TInput1, TInput2, SqlConnection, SqlTransaction> transactionalAction)
	        {
	            ExecWithTransaction((conn, trans) => transactionalAction(input1, input2, conn, trans));
	        }
	
	        protected TResult GetWithTransaction<TResult>(Func<SqlConnection, SqlTransaction, TResult> transactionalFunc)
	        {
	            return Get(conn =>
	            {
	                using (SqlTransaction trans = conn.BeginTransaction())
	                {
	                    TResult result = transactionalFunc(conn, trans);
	                    trans.Commit();
	                    return result;
	                }
	            });
	        }
	
	        protected TResult GetWithTransaction<TInput, TResult>(TInput input, Func<TInput, SqlConnection, SqlTransaction, TResult> transactionalFunc)
	        {
	            return GetWithTransaction((conn, trans) => transactionalFunc(input, conn, trans));
	        }
	
	        protected TResult GetWithTransaction<TInput1, TInput2, TResult>(TInput1 input1, TInput2 input2, Func<TInput1, TInput2, SqlConnection, SqlTransaction, TResult> transactionalFunc)
	        {
	            return GetWithTransaction((conn, trans) => transactionalFunc(input1, input2, conn, trans));
	        }
	
	        #endregion Methods
	    }

// Define other methods and classes here
