// Copyright ?2004, 2018, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.ComponentModel;
#if !RT
using System.Data;
using System.Data.Common;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;
#endif
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using MySql.Data.Common;
using System.Diagnostics;
using MySql.Data.MySqlClient.Properties;
using MySql.Data.MySqlClient.Replication;
#if NET_40_OR_GREATER
using System.Threading.Tasks;
using System.Threading;
#endif

namespace MySql.Data.MySqlClient
{
  /// <include file='docs/MySqlConnection.xml' path='docs/ClassSummary/*'/>
  public sealed partial class MySqlConnection : IDisposable
  {
    internal ConnectionState connectionState;
    internal Driver driver;
    internal bool hasBeenOpen;
    private SchemaProvider schemaProvider;
    private ProcedureCache procedureCache;
    private bool isInUse;
    private PerformanceMonitor perfMonitor;
#if !RT
    private ExceptionInterceptor exceptionInterceptor;
    internal CommandInterceptor commandInterceptor;
#endif
    private bool isKillQueryConnection;
    private string database;
    private int commandTimeout;

    /// <include file='docs/MySqlConnection.xml' path='docs/InfoMessage/*'/>
    public event MySqlInfoMessageEventHandler InfoMessage;

    private static Cache<string, MySqlConnectionStringBuilder> connectionStringCache =
      new Cache<string, MySqlConnectionStringBuilder>(0, 25);

    /// <include file='docs/MySqlConnection.xml' path='docs/DefaultCtor/*'/>
    public MySqlConnection()
    {
      //TODO: add event data to StateChange docs
      Settings = new MySqlConnectionStringBuilder();
      database = String.Empty;
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/Ctor1/*'/>
    public MySqlConnection(string connectionString)
      : this()
    {
      ConnectionString = connectionString;
    }

    #region Destructor
    ~MySqlConnection()
    {
      Dispose(false);
    }
    #endregion

    #region Interal Methods & Properties

    internal PerformanceMonitor PerfMonitor
    {
      get { return perfMonitor; }
    }

    internal ProcedureCache ProcedureCache
    {
      get { return procedureCache; }
    }

    internal MySqlConnectionStringBuilder Settings { get; private set; }

    internal MySqlDataReader Reader
    {
      get
      {
        if (driver == null)
          return null;
        return driver.reader;
      }
      set
      {
        driver.reader = value;
        isInUse = driver.reader != null;
      }
    }

    internal void OnInfoMessage(MySqlInfoMessageEventArgs args)
    {
      if (InfoMessage != null)
      {
        InfoMessage(this, args);
      }
    }

    internal bool SoftClosed
    {
      get
      {
#if !RT
        return (State == ConnectionState.Closed) &&
          driver != null &&
          driver.CurrentTransaction != null;
#else
        return false;            
#endif
      }
    }

    internal bool IsInUse
    {
      get { return isInUse; }
      set { isInUse = value; }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Returns the id of the server thread this connection is executing on
    /// </summary>
    [Browsable(false)]
    public int ServerThread
    {
      get { return driver.ThreadID; }
    }

    /// <summary>
    /// Gets the name of the MySQL server to which to connect.
    /// </summary>
    [Browsable(true)]
    public override string DataSource
    {
      get { return Settings.Server; }
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/ConnectionTimeout/*'/>
    [Browsable(true)]
    public override int ConnectionTimeout
    {
      get { return (int)Settings.ConnectionTimeout; }
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/Database/*'/>
    [Browsable(true)]
    public override string Database
    {
      get { return database; }
    }

    /// <summary>
    /// Indicates if this connection should use compression when communicating with the server.
    /// </summary>
    [Browsable(false)]
    public bool UseCompression
    {
      get { return Settings.UseCompression; }
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/State/*'/>
    [Browsable(false)]
    public override ConnectionState State
    {
      get { return connectionState; }
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/ServerVersion/*'/>
    [Browsable(false)]
    public override string ServerVersion
    {
      get { return driver.Version.ToString(); }
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/ConnectionString/*'/>
    [Editor("MySql.Data.MySqlClient.Design.ConnectionStringTypeEditor,MySqlClient.Design", typeof(UITypeEditor))]
    [Browsable(true)]
    [Category("Data")]
    [Description(
      "Information used to connect to a DataSource, such as 'Server=xxx;UserId=yyy;Password=zzz;Database=dbdb'.")]
    public override string ConnectionString
    {
      get
      {
        // Always return exactly what the user set.
        // Security-sensitive information may be removed.
        return Settings.GetConnectionString(!hasBeenOpen || Settings.PersistSecurityInfo);
      }
      set
      {
        if (State != ConnectionState.Closed)
          Throw(new MySqlException(
            "Not allowed to change the 'ConnectionString' property while the connection (state=" + State + ")."));

        MySqlConnectionStringBuilder newSettings;
        lock (connectionStringCache)
        {
          if (value == null)
            newSettings = new MySqlConnectionStringBuilder();
          else
          {
            newSettings = (MySqlConnectionStringBuilder)connectionStringCache[value];
            if (null == newSettings)
            {
              newSettings = new MySqlConnectionStringBuilder(value);
              connectionStringCache.Add(value, newSettings);
            }
          }
        }

        Settings = newSettings;

        if (Settings.Database != null && Settings.Database.Length > 0)
          this.database = Settings.Database;

        if (driver != null)
          driver.Settings = newSettings;
      }
    }

#if !__MonoCS__ && !RT

    protected override DbProviderFactory DbProviderFactory
    {
      get
      {
        return MySqlClientFactory.Instance;
      }
    }

#endif

    public bool IsPasswordExpired { get { return driver.IsPasswordExpired; } }

    #endregion

    partial void AssertPermissions();

    #region Transactions

#if !MONO && !RT
    /// <summary>
    /// Enlists in the specified transaction. 
    /// </summary>
    /// <param name="transaction">
    /// A reference to an existing <see cref="System.Transactions.Transaction"/> in which to enlist.
    /// </param>
    public override void EnlistTransaction(Transaction transaction)
    {
      // enlisting in the null transaction is a noop
      if (transaction == null)
        return;

      // guard against trying to enlist in more than one transaction
      if (driver.CurrentTransaction != null)
      {
        if (driver.CurrentTransaction.BaseTransaction == transaction)
          return;

        Throw(new MySqlException("Already enlisted"));
      }

      // now see if we need to swap out drivers.  We would need to do this since
      // we have to make sure all ops for a given transaction are done on the
      // same physical connection.
      Driver existingDriver = DriverTransactionManager.GetDriverInTransaction(transaction);
      if (existingDriver != null)
      {
        // we can't allow more than one driver to contribute to the same connection
        if (existingDriver.IsInActiveUse)
          Throw(new NotSupportedException(Resources.MultipleConnectionsInTransactionNotSupported));

        // there is an existing driver and it's not being currently used.
        // now we need to see if it is using the same connection string
        string text1 = existingDriver.Settings.ConnectionString;
        string text2 = Settings.ConnectionString;
        if (String.Compare(text1, text2, true) != 0)
          Throw(new NotSupportedException(Resources.MultipleConnectionsInTransactionNotSupported));

        // close existing driver
        // set this new driver as our existing driver
        CloseFully();
        driver = existingDriver;
      }

      if (driver.CurrentTransaction == null)
      {
        MySqlPromotableTransaction t = new MySqlPromotableTransaction(this, transaction);
        if (!transaction.EnlistPromotableSinglePhase(t))
          Throw(new NotSupportedException(Resources.DistributedTxnNotSupported));

        driver.CurrentTransaction = t;
        DriverTransactionManager.SetDriverInTransaction(driver);
        driver.IsInActiveUse = true;
      }
    }
#endif

    /// <include file='docs/MySqlConnection.xml' path='docs/BeginTransaction/*'/>
    public new MySqlTransaction BeginTransaction()
    {
      return BeginTransaction(IsolationLevel.RepeatableRead);
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/BeginTransaction1/*'/>
    public new MySqlTransaction BeginTransaction(IsolationLevel iso)
    {
      //TODO: check note in help
      if (State != ConnectionState.Open)
        Throw(new InvalidOperationException(Resources.ConnectionNotOpen));

      // First check to see if we are in a current transaction
      if (driver.HasStatus(ServerStatusFlags.InTransaction))
        Throw(new InvalidOperationException(Resources.NoNestedTransactions));

      MySqlTransaction t = new MySqlTransaction(this, iso);

      MySqlCommand cmd = new MySqlCommand("", this);

      cmd.CommandText = "SET SESSION TRANSACTION ISOLATION LEVEL ";
      switch (iso)
      {
        case IsolationLevel.ReadCommitted:
          cmd.CommandText += "READ COMMITTED";
          break;
        case IsolationLevel.ReadUncommitted:
          cmd.CommandText += "READ UNCOMMITTED";
          break;
        case IsolationLevel.RepeatableRead:
          cmd.CommandText += "REPEATABLE READ";
          break;
        case IsolationLevel.Serializable:
          cmd.CommandText += "SERIALIZABLE";
          break;
        case IsolationLevel.Chaos:
          Throw(new NotSupportedException(Resources.ChaosNotSupported));
          break;
        case IsolationLevel.Snapshot:
          Throw(new NotSupportedException(Resources.SnapshotNotSupported));
          break;
      }

      cmd.ExecuteNonQuery();

      cmd.CommandText = "BEGIN";
      cmd.ExecuteNonQuery();

      return t;
    }

    #endregion

    /// <include file='docs/MySqlConnection.xml' path='docs/ChangeDatabase/*'/>
    public override void ChangeDatabase(string databaseName)
    {
      if (databaseName == null || databaseName.Trim().Length == 0)
        Throw(new ArgumentException(Resources.ParameterIsInvalid, "databaseName"));

      if (State != ConnectionState.Open)
        Throw(new InvalidOperationException(Resources.ConnectionNotOpen));

      // This lock  prevents promotable transaction rollback to run
      // in parallel
      lock (driver)
      {
#if !RT
        if (Transaction.Current != null &&
          Transaction.Current.TransactionInformation.Status == TransactionStatus.Aborted)
        {
          Throw(new TransactionAbortedException());
        }
#endif
        // We use default command timeout for SetDatabase
        using (new CommandTimer(this, (int)Settings.DefaultCommandTimeout))
        {
          driver.SetDatabase(databaseName);
        }
      }
      this.database = databaseName;
    }

    internal void SetState(ConnectionState newConnectionState, bool broadcast)
    {
      if (newConnectionState == connectionState && !broadcast)
        return;
      ConnectionState oldConnectionState = connectionState;
      connectionState = newConnectionState;
      if (broadcast)
        OnStateChange(new StateChangeEventArgs(oldConnectionState, connectionState));
    }

    /// <summary>
    /// Ping
    /// </summary>
    /// <returns></returns>
    public bool Ping()
    {
      if (Reader != null)
        Throw(new MySqlException(Resources.DataReaderOpen));
      if (driver != null && driver.Ping())
        return true;
      driver = null;
      SetState(ConnectionState.Closed, true);
      return false;
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/Open/*'/>
    public override void Open()
    {
      if (State == ConnectionState.Open)
        Throw(new InvalidOperationException(Resources.ConnectionAlreadyOpen));

#if !RT
      // start up our interceptors
      exceptionInterceptor = new ExceptionInterceptor(this);
      commandInterceptor = new CommandInterceptor(this);
#endif

      SetState(ConnectionState.Connecting, true);

      AssertPermissions();

#if !RT
      // if we are auto enlisting in a current transaction, then we will be
      // treating the connection as pooled
      if (Settings.AutoEnlist && Transaction.Current != null)
      {
        driver = DriverTransactionManager.GetDriverInTransaction(Transaction.Current);
        if (driver != null &&
          (driver.IsInActiveUse ||
          !driver.Settings.EquivalentTo(this.Settings)))
          Throw(new NotSupportedException(Resources.MultipleConnectionsInTransactionNotSupported));
      }
#endif

      try
      {
        MySqlConnectionStringBuilder currentSettings = Settings;

        // Load balancing 
        if (ReplicationManager.IsReplicationGroup(Settings.Server))
        {
          if (driver == null)
          {
            ReplicationManager.GetNewConnection(Settings.Server, false, this);
          }
          else
            currentSettings = driver.Settings;
        }

        if (Settings.Pooling)
        {
          MySqlPool pool = MySqlPoolManager.GetPool(currentSettings);
          if (driver == null || !driver.IsOpen)
            driver = pool.GetConnection();
          procedureCache = pool.ProcedureCache;

        }
        else
        {
          if (driver == null || !driver.IsOpen)
            driver = Driver.Create(currentSettings);
          procedureCache = new ProcedureCache((int)Settings.ProcedureCacheSize);
        }
      }
      catch (Exception ex)
      {
        SetState(ConnectionState.Closed, true);
        throw;
      }

      // if the user is using old syntax, let them know
      if (driver.Settings.UseOldSyntax)
        MySqlTrace.LogWarning(ServerThread,
          "You are using old syntax that will be removed in future versions");

      SetState(ConnectionState.Open, false);
      driver.Configure(this);

      if (!(driver.SupportsPasswordExpiration && driver.IsPasswordExpired))
      {
        if (Settings.Database != null && Settings.Database != String.Empty)
          ChangeDatabase(Settings.Database);
      }

      // setup our schema provider
      schemaProvider = new ISSchemaProvider(this);
      perfMonitor = new PerformanceMonitor(this);

      // if we are opening up inside a current transaction, then autoenlist
      // TODO: control this with a connection string option
#if !MONO && !RT
      if (Transaction.Current != null && Settings.AutoEnlist)
        EnlistTransaction(Transaction.Current);
#endif

      hasBeenOpen = true;
      SetState(ConnectionState.Open, true);
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/CreateCommand/*'/>
    public new MySqlCommand CreateCommand()
    {
      // Return a new instance of a command object.
      MySqlCommand c = new MySqlCommand();
      c.Connection = this;
      return c;
    }

    /// <summary>
    /// Creates a new MySqlConnection object with the exact same ConnectionString value
    /// </summary>
    /// <returns>A cloned MySqlConnection object</returns>
    public object Clone()
    {
      MySqlConnection clone = new MySqlConnection();
      string connectionString = Settings.ConnectionString;
      if (connectionString != null)
        clone.ConnectionString = connectionString;
      return clone;
    }

    internal void Abort()
    {
      try
      {
        driver.Close();
      }
      catch (Exception ex)
      {
        MySqlTrace.LogWarning(ServerThread, String.Concat("Error occurred aborting the connection. Exception was: ", ex.Message));
      }
      finally
      {
        this.isInUse = false;
      }
      SetState(ConnectionState.Closed, true);
    }

    internal void CloseFully()
    {
      if (Settings.Pooling && driver.IsOpen)
      {
        // if we are in a transaction, roll it back
        if (driver.HasStatus(ServerStatusFlags.InTransaction))
        {
          MySqlTransaction t = new MySqlTransaction(this, IsolationLevel.Unspecified);
          t.Rollback();
        }

        MySqlPoolManager.ReleaseConnection(driver);
      }
      else
        driver.Close();
      driver = null;
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/Close/*'/>
    public override void Close()
    {
      if (driver != null)
        driver.IsPasswordExpired = false;

      if (State == ConnectionState.Closed) return;

      if (Reader != null)
        Reader.Close();

      // if the reader was opened with CloseConnection then driver
      // will be null on the second time through
      if (driver != null)
      {
#if !RT
        if (driver.CurrentTransaction == null)
#endif
          CloseFully();
#if !RT
        else
          driver.IsInActiveUse = false;
#endif
      }

      SetState(ConnectionState.Closed, true);
    }

    internal string CurrentDatabase()
    {
      if (Database != null && Database.Length > 0)
        return Database;
      MySqlCommand cmd = new MySqlCommand("SELECT database()", this);
      return cmd.ExecuteScalar().ToString();
    }



    internal void HandleTimeoutOrThreadAbort(Exception ex)
    {
      bool isFatal = false;

      if (isKillQueryConnection)
      {
        // Special connection started to cancel a query.
        // Abort will prevent recursive connection spawning
        Abort();
        if (ex is TimeoutException)
        {
          Throw(new MySqlException(Resources.Timeout, true, ex));
        }
        else
        {
          return;
        }
      }

      try
      {

        // Do a fast cancel.The reason behind small values for connection
        // and command timeout is that we do not want user to wait longer
        // after command has already expired.
        // Microsoft's SqlClient seems to be using 5 seconds timeouts 
        // here as well.
        // Read the  error packet with "interrupted" message.
        CancelQuery(5);
        driver.ResetTimeout(5000);
        if (Reader != null)
        {
          Reader.Close();
          Reader = null;
        }
      }
      catch (Exception ex2)
      {
        MySqlTrace.LogWarning(ServerThread, "Could not kill query, " +
          " aborting connection. Exception was " + ex2.Message);
        Abort();
        isFatal = true;
      }
      if (ex is TimeoutException)
      {
        Throw(new MySqlException(Resources.Timeout, isFatal, ex));
      }
    }

    public void CancelQuery(int timeout)
    {
      MySqlConnectionStringBuilder cb = new MySqlConnectionStringBuilder(
        Settings.ConnectionString);
      cb.Pooling = false;
      cb.AutoEnlist = false;
      cb.ConnectionTimeout = (uint)timeout;

      using (MySqlConnection c = new MySqlConnection(cb.ConnectionString))
      {
        c.isKillQueryConnection = true;
        c.Open();
        string commandText = "KILL QUERY " + ServerThread;
        MySqlCommand cmd = new MySqlCommand(commandText, c);
        cmd.CommandTimeout = timeout;
        cmd.ExecuteNonQuery();
      }
    }

    #region Routines for timeout support.

    // Problem description:
    // Sometimes, ExecuteReader is called recursively. This is the case if
    // command behaviors are used and we issue "set sql_select_limit" 
    // before and after command. This is also the case with prepared 
    // statements , where we set session variables. In these situations, we 
    // have to prevent  recursive ExecuteReader calls from overwriting 
    // timeouts set by the top level command.

    // To solve the problem, SetCommandTimeout() and ClearCommandTimeout() are 
    // introduced . Query timeout here is  "sticky", that is once set with 
    // SetCommandTimeout, it only be overwritten after ClearCommandTimeout 
    // (SetCommandTimeout would return false if it timeout has not been 
    // cleared).

    // The proposed usage pattern of there routines is following: 
    // When timed operations starts, issue SetCommandTimeout(). When it 
    // finishes, issue ClearCommandTimeout(), but _only_ if call to 
    // SetCommandTimeout() was successful.


    /// <summary>
    /// Sets query timeout. If timeout has been set prior and not
    /// yet cleared ClearCommandTimeout(), it has no effect.
    /// </summary>
    /// <param name="value">timeout in seconds</param>
    /// <returns>true if </returns>
    internal bool SetCommandTimeout(int value)
    {
      if (!hasBeenOpen)
        // Connection timeout is handled by driver
        return false;

      if (commandTimeout != 0)
        // someone is trying to set a timeout while command is already
        // running. It could be for example recursive call to ExecuteReader
        // Ignore the request, as only top-level (non-recursive commands)
        // can set timeouts.
        return false;

      if (driver == null)
        return false;

      commandTimeout = value;
      driver.ResetTimeout(commandTimeout * 1000);
      return true;
    }

    /// <summary>
    /// Clears query timeout, allowing next SetCommandTimeout() to succeed.
    /// </summary>
    internal void ClearCommandTimeout()
    {
      if (!hasBeenOpen)
        return;
      commandTimeout = 0;
      if (driver != null)
      {
        driver.ResetTimeout(0);
      }
    }
    #endregion

    public MySqlSchemaCollection GetSchemaCollection(string collectionName, string[] restrictionValues)
    {
      if (collectionName == null)
        collectionName = SchemaProvider.MetaCollection;

      string[] restrictions = schemaProvider.CleanRestrictions(restrictionValues);
      MySqlSchemaCollection c = schemaProvider.GetSchema(collectionName, restrictions);
      return c;
    }

    #region Pool Routines

    /// <include file='docs/MySqlConnection.xml' path='docs/ClearPool/*'/>
    public static void ClearPool(MySqlConnection connection)
    {
      MySqlPoolManager.ClearPool(connection.Settings);
    }

    /// <include file='docs/MySqlConnection.xml' path='docs/ClearAllPools/*'/>
    public static void ClearAllPools()
    {
      MySqlPoolManager.ClearAllPools();
    }

    #endregion

    internal void Throw(Exception ex)
    {
#if !RT
      if (exceptionInterceptor == null)
        throw ex;
      exceptionInterceptor.Throw(ex);
#else
      throw ex;
#endif
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

#if NET_40_OR_GREATER
    #region Async
    /// <summary>
    /// Async version of BeginTransaction
    /// </summary>
    /// <returns>An object representing the new transaction.</returns>
    public Task<MySqlTransaction> BeginTransactionAsync()
    {
      return BeginTransactionAsync(IsolationLevel.RepeatableRead, CancellationToken.None);
    }

    public Task<MySqlTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
      return BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
    }

    /// <summary>
    /// Async version of BeginTransaction
    /// </summary>
    /// <param name="iso">The isolation level under which the transaction should run. </param>
    /// <returns>An object representing the new transaction.</returns>
    public Task<MySqlTransaction> BeginTransactionAsync(IsolationLevel iso)
    {
      return BeginTransactionAsync(iso, CancellationToken.None);
    }

    public Task<MySqlTransaction> BeginTransactionAsync(IsolationLevel iso, CancellationToken cancellationToken)
    {
      var result = new TaskCompletionSource<MySqlTransaction>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          MySqlTransaction tranResult = BeginTransaction(iso);
          result.SetResult(tranResult);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }

      return result.Task;
    }

    public Task ChangeDataBaseAsync(string databaseName)
    {
      return ChangeDataBaseAsync(databaseName, CancellationToken.None);
    }

    /// <summary>
    /// Async version of ChangeDataBase
    /// </summary>
    /// <param name="databaseName">The name of the database to use.</param>
    /// <returns></returns>
    public Task ChangeDataBaseAsync(string databaseName, CancellationToken cancellationToken)
    {
      var result = new TaskCompletionSource<bool>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          ChangeDatabase(databaseName);
          result.SetResult(true);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      return result.Task;
    }

    ///// <summary>
    ///// Async version of Open
    ///// </summary>
    ///// <returns></returns>
    //public Task OpenAsync()
    //{
    //  return Task.Run(() =>
    //  {
    //    Open();
    //  });
    //}

    /// <summary>
    /// Async version of Close
    /// </summary>
    /// <returns></returns>
    public Task CloseAsync()
    {
      return CloseAsync(CancellationToken.None);
    }

    public Task CloseAsync(CancellationToken cancellationToken)
    {
      var result = new TaskCompletionSource<bool>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          Close();
          result.SetResult(true);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else 
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    /// <summary>
    /// Async version of ClearPool
    /// </summary>
    /// <param name="connection">The connection associated with the pool to be cleared.</param>
    /// <returns></returns>
    public Task ClearPoolAsync(MySqlConnection connection)
    {
      return ClearPoolAsync(connection, CancellationToken.None);
    }

    public Task ClearPoolAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
      var result = new TaskCompletionSource<bool>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          ClearPool(connection);
          result.SetResult(true);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }

    /// <summary>
    /// Async version of ClearAllPools
    /// </summary>
    /// <returns></returns>
    public Task ClearAllPoolsAsync()
    {
      return ClearAllPoolsAsync(CancellationToken.None);
    }

    public Task ClearAllPoolsAsync(CancellationToken cancellationToken)
    {
      var result = new TaskCompletionSource<bool>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          ClearAllPools();
          result.SetResult(true);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }
    /// <summary>
    /// Async version of GetSchemaCollection
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="restrictionValues">Values to restrict</param>
    /// <returns>A schema collection</returns>
    public Task<MySqlSchemaCollection> GetSchemaCollectionAsync(string collectionName, string[] restrictionValues)
    {
      return GetSchemaCollectionAsync(collectionName, restrictionValues, CancellationToken.None);
    }

    public Task<MySqlSchemaCollection> GetSchemaCollectionAsync(string collectionName, string[] restrictionValues, CancellationToken cancellationToken)
    {
      var result = new TaskCompletionSource<MySqlSchemaCollection>();
      if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
      {
        try
        {
          var schema = GetSchemaCollection(collectionName, restrictionValues);
          result.SetResult(schema);
        }
        catch (Exception ex)
        {
          result.SetException(ex);
        }
      }
      else
      {
        result.SetCanceled();
      }
      return result.Task;
    }
    #endregion
#endif
  }

  /// <summary>
  /// Represents the method that will handle the <see cref="MySqlConnection.InfoMessage"/> event of a 
  /// <see cref="MySqlConnection"/>.
  /// </summary>
  public delegate void MySqlInfoMessageEventHandler(object sender, MySqlInfoMessageEventArgs args);

  /// <summary>
  /// Provides data for the InfoMessage event. This class cannot be inherited.
  /// </summary>
  public class MySqlInfoMessageEventArgs : EventArgs
  {
    /// <summary>
    /// 
    /// </summary>
    public MySqlError[] errors;
  }

  /// <summary>
  /// IDisposable wrapper around SetCommandTimeout and ClearCommandTimeout
  /// functionality
  /// </summary>
  internal class CommandTimer : IDisposable
  {
    bool timeoutSet;
    MySqlConnection connection;

    public CommandTimer(MySqlConnection connection, int timeout)
    {
      this.connection = connection;
      if (connection != null)
      {
        timeoutSet = connection.SetCommandTimeout(timeout);
      }
    }

    #region IDisposable Members
    public void Dispose()
    {
      if (timeoutSet)
      {
        timeoutSet = false;
        connection.ClearCommandTimeout();
        connection = null;
      }
    }
    #endregion
  }
}
