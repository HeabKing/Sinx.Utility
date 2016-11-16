using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;

namespace Sinx.Utility.Extension
{
	/// <summary>
	/// 基于Dapper给IDbConnection提供拓展, 支持实体下的基本增删改查
	/// </summary>
	// ReSharper disable once InconsistentNaming
    public static class IDbConnectionEx
    {
		// Model => field = @field, field2 = @field2...
		private static readonly Func<IEnumerable<string>, string, string> TempDelegate =
			(ie, salt) => ie
				.Aggregate("", (c, i) => c + i + " = @" + i + (salt ?? "") + ",")
				.TrimEnd(',');

	    /// <summary>
	    /// 获取指定条件下的数据集合
	    /// </summary>
	    /// <param name="db">IDbConnection</param>
	    /// <param name="model">指定条件</param>
	    /// <param name="defaultFields">按照默认值查询的字段</param>
	    /// <returns>没有值返回空集合</returns>
	    // ReSharper disable once MemberCanBePrivate.Global
	    public static Task<IEnumerable<T>> GetAsync<T>(this IDbConnection db, T model, params string[] defaultFields)
		{
			var fileds = model.GetAssignedProperties().ToList();
			fileds.AddRange(defaultFields.Select(item => new KeyValuePair<string, object>(item, typeof(T).GetTypeInfo().GetDeclaredProperty(item).GetValue(model))));
			if (!fileds.Any())
			{
				throw new ArgumentException("请输入最少一个查询条件");
			}
			// TODO Can here nameof(T)?
			var sql = $@"
				SELECT *
				FROM {typeof(T).Name}
				WHERE 1 = 1";
			sql = fileds.Aggregate(sql, (c, i) => c + $" AND {i.Key} = @{i.Key} ");
			return db.QueryAsync<T>(sql, model);
		}

		/// <summary>
		/// 根据Id查询实体
		/// </summary>
		/// <param name="db">IDbConnection</param>
		/// <param name="id">Id条件</param>
		/// <returns></returns>
		public static async Task<T> GetAsync<T>(this IDbConnection db, int id)
		{
			var idInfo = typeof(T).GetTypeInfo().GetDeclaredProperty("Id");
			if (idInfo == null)
			{
				throw new ArgumentException("参数没有基于约定将主键的属性名设置为Id");
			}
			var insModel = Activator.CreateInstance(typeof(T));
			idInfo.SetValue(insModel, id);
			var models = await db.GetAsync((T)insModel);
			return models.SingleOrDefault();
		}

		/// <summary>
		/// 添加一条数据
		/// </summary>
		/// <param name="db">IDbConnection</param>
		/// <param name="model">只将非默认值的字段添加到数据库</param>
		/// <returns>添加数据的Id</returns>
		public static Task<int> InsertAsync<T>(this IDbConnection db, T model) where T : class
		{
			var assignedPropertyKeys = model.GetAssignedProperties().Select(m => m.Key).ToList();
			string sql = $@"
				INSERT INTO {typeof(T).Name}
					({string.Join(",", assignedPropertyKeys)})
				VALUES
					(@{string.Join(",@", assignedPropertyKeys)});
				SELECT @@IDENTITY;";
			return db.QueryFirstAsync<int>(sql, model);
		}

		/// <summary>
		/// 更新指定的实体
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="db"></param>
		/// <param name="model">实体数据</param>
		/// <param name="where">查询条件</param>
		/// <returns></returns>
		// ReSharper disable once MemberCanBePrivate.Global
		public static Task<int> UpdateAsync<T>(this IDbConnection db, T model, T where) where T : class
		{
			var assignedProperty = model.GetAssignedProperties().ToList();
			var assignedPropertyKeys = assignedProperty.Select(m => m.Key).ToList();
			var assignedPropertyFromWhere = where.GetAssignedProperties().ToList();
			var assignedPropertyKeysFromWhere = assignedPropertyFromWhere.Select(m => m.Key).ToList();

			string sql = $@"
				UPDATE {typeof(T).Name}
				SET {TempDelegate(assignedPropertyKeys, null)}
				WHERE {TempDelegate(assignedPropertyKeysFromWhere, "2")}";
			int count;
			using (var cmd = db.CreateCommand())
			{
				cmd.CommandText = sql;
				foreach (var item in assignedProperty)
				{
					cmd.Parameters.Add(new SqlParameter(item.Key, item.Value));
				}
				foreach (var item in assignedPropertyFromWhere)
				{
					cmd.Parameters.Add(new SqlParameter(item.Key + 2, item.Value));
				}
				db.Open();
				count = Convert.ToInt32(cmd.ExecuteNonQuery());
				db.Close();
			}
			return Task.FromResult(count);
		}

		/// <summary>
		/// 更新指定数据, 条件为Id
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="db"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		public static Task<int> UpdateAsync<T>(this IDbConnection db, T model) where T : class
		{
			var id = model.GetType().GetTypeInfo().GetDeclaredProperty("Id");
			if (id == null)
			{
				throw new ArgumentException("参数没有基于约定将主键的属性名设置为Id");
			}
			var value = id.GetValue(model);
			if (Convert.ToInt32(value) <= 0)
			{
				throw new ArgumentException("参数没有指定Id的值");
			}
			id.SetValue(model, 0);
			var where = Activator.CreateInstance(typeof(T));
			id.SetValue(where, value);
			return db.UpdateAsync(model, (T)where);
		}

		/// <summary>
		/// 删除指定对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="db"></param>
		/// <param name="where"></param>
		/// <returns></returns>
		public static Task<int> DeleteAsync<T>(IDbConnection db, T where) where T : class
		{
			var assignedPropertyKeys = where.GetAssignedProperties().Select(m => m.Key);
			string sql = $@"DELETE FROM {typeof(T).Name} WHERE {TempDelegate(assignedPropertyKeys, null)}";
			return db.ExecuteAsync(sql, where);
		}
	}
}
