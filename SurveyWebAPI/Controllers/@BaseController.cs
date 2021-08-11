using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace SurveyWebAPI.Controllers
{
	public class Tmp
	{
		public Guid guid;
		public DateTime dtc;
		public Type type;
		public Object resources;

		public Tmp( Guid guid, Object res )
		{
			this.guid = guid;
			this.dtc = DateTime.Now;
			this.type = res.GetType();

			this.resources = res;
		}
	}

	public class BaseController : ControllerBase
	{
		static List<Tmp> repo;

		static BaseController()
		{
			repo = new List<Tmp>();
			repo.Add( new Tmp( Guid.NewGuid(), new { Value = 1 } ) );
			repo.Add( new Tmp( Guid.NewGuid(), new { Value = 2 } ) );
			repo.Add( new Tmp( Guid.NewGuid(), new { Value = 3 } ) );
		}

		protected void SetValidRepoBy<T>( Guid id, T item ) { repo.Add( new Tmp( id, item ) ); }

		protected T GetValidFromRepo<T>( Guid id ) where T : class
		{
			foreach ( var tmp in repo )
			{
				if ( !tmp.guid.Equals( id ) ) continue;

				repo.Remove( tmp );
				return (T) tmp.resources;
			}

			return null;
		}
	}

	public static class BaseControllerExtensions
	{
		public static SqlGuid ValidGuid( this String value )
		{
			if ( Regex.IsMatch( value, "[^\\#\\;]+" ) ) throw new Exception( $"not valid guid: {value}" );
			return SqlGuid.Parse( value );
		}

		const string PATTERN = "([A-Z]|[a-z]|[0-9]|[]|\\d|\\s|[+,-\\\\.*()_\"'|:<>@!#$%^&={}]|[\u4e00-\u9fa5])";


		public static String Valid( this Object value )
		{
			return value == null ? String.Empty : value.ToString().Valid();
		}
		public static String Valid( this String value )
		{
			if ( String.IsNullOrWhiteSpace( value ) ) return String.Empty;

			var buffer = String.Empty;

			var regex = new Regex( PATTERN, RegexOptions.IgnoreCase );
			var r = regex.Matches( value );
			for ( var idx = 0; idx < r.Count; idx++ ) buffer += r[idx].Value;


			return buffer;
		}

		public static Boolean ValidBit( this JToken? value, bool defaultValue = false )
		{
			var v = value == null || !value.HasValues ? defaultValue : value.ToString() as Object;

			return v != null ? ValidBit( v.ToString() ) : defaultValue;
		}

		public static Boolean ValidInt16Bit( this String srcV )
		{
			var buffer = String.Empty;

			var regex = new Regex( "[0-9]" );
			var r = regex.Matches( String.IsNullOrWhiteSpace( srcV ) ? "0" : srcV );

			for ( var idx = 0; idx < r.Count; idx++ ) buffer += r[idx].Value;

			var vInt16 = Convert.ToInt16( buffer.Length > 0 ? buffer : "0" );
			//Console.WriteLine( $"[toInt16] buffer({buffer.Length})[{buffer}] srcV[{srcV}]" );

			return Convert.ToBoolean( vInt16 );
		}

		public static DateTime ValidDateTime( this String srcV )
		{
			var buffer = String.Empty;

			var regex = new Regex( "[0-9]" );
			var r = regex.Matches( String.IsNullOrWhiteSpace( srcV ) ? "0" : srcV );

			for ( var idx = 0; idx < r.Count; idx++ ) buffer += r[idx].Value;

			if ( buffer.Length <= 0 ) throw new Exception( $"異常的日期格式: {buffer} 原始資料[{srcV}]" );

			return Convert.ToDateTime( buffer );
		}


		public static Boolean ValidBit( this String value, Boolean defaultValue = false )
		{
			var buffer = String.Empty;

			var regex = new Regex( PATTERN, RegexOptions.IgnoreCase );
			var r = regex.Matches( String.IsNullOrWhiteSpace( value ) ? defaultValue.ToString() : value );

			for ( var idx = 0; idx < r.Count; idx++ ) buffer += r[idx].Value;

			return Convert.ToBoolean( buffer );
		}

		public static Int32 ValidInt( this int value ) { return ValidInt( value.ToString() ); }

		public static Int32 ValidInt( this Object value, String defaultValue = "0" )
		{
			var v = value == null ? defaultValue : value.ToString();
			return ValidInt( v );
		}

		public static Int32 ValidInt( this JToken value, int defaultValue = 0 )
		{
			var v = value == null || !value.HasValues ? defaultValue : value.ToString() as Object;

			return v != null ? ValidInt( v.ToString() ) : defaultValue;
		}

		public static Int32 ValidInt( this String value, String defaultValue = "0" )
		{
			var buffer = String.Empty;

			var regex = new Regex( "(0-9|\\-)" );
			var r = regex.Matches( String.IsNullOrWhiteSpace( value ) ? defaultValue : value );

			for ( var idx = 0; idx < r.Count; idx++ ) buffer += r[idx].Value;

			Int32.TryParse( buffer, out var ret );

			return ret;
		}

		public static Object ValidStrOrDBNull( this JToken? value )
		{
			var v = ( value == null || !value.HasValues ? String.Empty : value.ToString() ).Valid();

			return String.IsNullOrWhiteSpace( v ) ? DBNull.Value : (Object) v;
		}
	}
}
