<Query Kind="Program">
  <Connection>
    <ID>4e94eacc-a31d-4687-947b-e4c9804c895a</ID>
    <Persist>true</Persist>
    <Server>(local)</Server>
    <IncludeSystemObjects>true</IncludeSystemObjects>
    <Database>PmRewriteApplicationDatabase</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

void Main()
{

	// for pushing local to lisa
	var app = this.Connection.ConnectionString.Split(new[] { ';' }).Select(a => a.Split(new[] { '='})).Last().Last();
	
	var csLisa = @"Data Source=192.168.0.190;User ID=sa;Password=;Initial Catalog=ApplicationDatabase;App=" + app; 
	//@"Data Source=(local);Integrated Security=SSPI;Initial Catalog=PmRewriteApplicationDatabase"
	var sb = new StringBuilder();
	try
	using (var cn = new SqlConnection(csLisa))
	{
		cn.Open();
		var @params = typeof(Codes).Dump("codes").GetFields().Dump("Code fields");
		var paramNames = @params.Select(p => p.Name);
		using (var cmd = new SqlCommand("set identity_insert codes ON",cn))
		{
			cmd.ExecuteNonQuery();
		}
		var cmdText = "insert into codes(" + paramNames.Delimit(",") + ") values (" + paramNames.Select(p => "@" + p).Delimit(",") +")";
		cmdText.Dump();
		using (var cmd = new SqlCommand(cmdText,cn))
		{
			foreach (var p in @params)
			{
				var param = cmd.CreateParameter();
				param.ParameterName= ("@"+p.Name);
				cmd.Parameters.Add(param);
            }
				
			foreach (var c in Codes)
			{
				var cmdLiteral = cmdText;
				
				foreach (var p in @params)
				{
					var value = p.GetValue(c);
					var sqlParam = cmd.Parameters["@"+p.Name];
					sqlParam.Value = value == null? System.DBNull.Value:value;
					var valueText = GetLiteral(p.FieldType, sqlParam.Value);
					cmdLiteral = cmdLiteral.Replace("@"+p.Name,valueText);
				}
				sb.AppendLine(cmdLiteral);
				cmd.ExecuteNonQuery();
			}
		}

	}
    finally
	{
		sb.ToString().OnDemand().Dump("raw command text log");
	}
	
	
}

public static string GetLiteral(Type type, object obj)
{
	if (type.IsValueType)
	return obj.ToString();
	if(type == typeof(string))
		return obj == null || obj == DBNull.Value ? "null":"\"" + obj.ToString().Replace("\"","\"\"")+"\"";
	throw new InvalidOperationException(type.Name);
}