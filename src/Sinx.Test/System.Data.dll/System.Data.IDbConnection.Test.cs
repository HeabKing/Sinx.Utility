using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Sinx.Test.System.Data.dll
{
    public class IDbConnectionTest
    {
	    private readonly IDbConnection _db = new SqlConnection("Data Source=qds150599512.my3w.com;Initial Catalog=qds150599512_db;User Id=qds150599512;Password=he394899990;");

		[Fact]
	    public void GetAsyncTest()
	    {
			// Ensure that the connection is opened (otherwise executing the command will fail)
			ConnectionState originalState = _db.State;
			if (originalState != ConnectionState.Open)
				_db.Open();	// TODO what if the _db is busy and can't open ? new a new instance?
			try
			{
				IDbCommand cmd = _db.CreateCommand();
				cmd.CommandText = "SELECT * FROM dbo.Z_Content";
				var r = cmd.ExecuteScalar();
				Assert.True(Convert.ToInt32(r) > 0);
			}
			finally
			{
				// Close the connection if that's how we got it
				if (originalState == ConnectionState.Closed)	// TODO think if not closed
					_db.Close();	// TODO 
				// 如果SqlConnection离开了作用域, 他并不会关闭, 所以必须明确调用Close()/Dispose.Close()(这两个方法相同)
			}
		}

	    private class Z_Cotent
	    {
			public int Id { get; set; }
			public string Content { get; set; }
		}
    }
}
