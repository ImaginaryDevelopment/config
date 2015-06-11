<Query Kind="Program" />

void Main()
{
}
	public abstract class BusinessObject<T> where T : BusinessObject<T>, new()
	    {
	        #region Member Variables
	
	        /// <summary>
	        /// Object that acts as a lock when retrieving or creating the singleton instance.
	        /// </summary>
	        private static object singletonLock = new object();
	
	        /// <summary>
	        /// Singleton instance of this business object with default connection string.
	        /// </summary>
	        private static T instance;
	
	        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
	        #endregion Member Variables
	
	        #region Properties
	
	        /// <summary>
	        /// Gets the connection string.  
	        /// </summary>
	        /// <value>The connection string.</value>
	        public virtual string ConnectionString { get; protected set; }
	
	        /// <summary>
	        /// Default connection string for business object.
	        /// </summary>
	        protected virtual string DefaultConnectionString { get { return ConfigurationManager.ConnectionStrings["LocalSqlConnection"].ConnectionString; } }
	
	        /// <summary>
	        /// Creates or retrieves the singleton instance of this business object.
	        /// </summary>
	        public static T Instance { get { return GetInstance(); } }
	
	        protected static log4net.ILog Log { get { return log; } }
	
	        #endregion Properties
	
	        #region Constructor
	
	        /// <summary>
	        /// Initializes a new instance of the <see cref="BusinessObject"/> class.
	        /// </summary>
	        protected BusinessObject()
	        {
	            ConnectionString = DefaultConnectionString;
	        }
	
	        #endregion Constructor
	
	        #region Methods
	
	        /// <summary>
	        /// Returns singleton business object using default connection string.
	        /// </summary>
	        public static T GetInstance()
	        {
	            if (instance != null)
	                return instance;
	
	            // If instance has not been created, create new instance (with thread-safe lock).
	            lock (singletonLock)
	            {
	                if (instance == null)
	                    instance = new T();
	            }
	
	            return instance;
	        }
	
	        /// <summary>
	        /// Returns new instance of business object using custom connection string.
	        /// </summary>
	        public static T GetInstance(string connectionString)
	        {
	            return new T() { ConnectionString = connectionString };
	        }
	
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
