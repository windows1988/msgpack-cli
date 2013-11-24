﻿#region -- License Terms --
//
// MessagePack for CLI
//
// Copyright (C) 2010-2012 FUJIWARA, Yusuke
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
#endregion -- License Terms --

using System;
#if SILVERLIGHT || NETFX_35
using System.Collections.Generic;
#else
using System.Collections.Concurrent;
#endif
using System.Diagnostics.Contracts;
#if NETFX_CORE
using System.Linq.Expressions;
#endif
using MsgPack.Serialization.AbstractSerializers;
#if !NETFX_CORE
#if !SILVERLIGHT
using MsgPack.Serialization.CodeDomSerializers;
#endif
using MsgPack.Serialization.EmittingSerializers;
#endif
#if !WINDOWS_PHONE && !NETFX_35
using MsgPack.Serialization.ExpressionSerializers;
using System.Globalization;
#endif

namespace MsgPack.Serialization
{
	/// <summary>
	///		Defines entry points for <see cref="MessagePackSerializer{T}"/> usage.
	/// </summary>
	public static class MessagePackSerializer
	{
		/// <summary>
		///		Creates new <see cref="MessagePackSerializer{T}"/> instance with <see cref="SerializationContext.Default"/>.
		/// </summary>
		/// <typeparam name="T">Target type.</typeparam>
		/// <returns>
		///		New <see cref="MessagePackSerializer{T}"/> instance to serialize/deserialize the object tree which the top is <typeparamref name="T"/>.
		/// </returns>
		public static MessagePackSerializer<T> Create<T>()
		{
			Contract.Ensures( Contract.Result<MessagePackSerializer<T>>() != null );

			return Create<T>( SerializationContext.Default );
		}

		/// <summary>
		///		Creates new <see cref="MessagePackSerializer{T}"/> instance with specified <see cref="SerializationContext"/>.
		/// </summary>
		/// <typeparam name="T">Target type.</typeparam>
		/// <param name="context">
		///		<see cref="SerializationContext"/> to store known/created serializers.
		/// </param>
		/// <returns>
		///		New <see cref="MessagePackSerializer{T}"/> instance to serialize/deserialize the object tree which the top is <typeparamref name="T"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="context"/> is <c>null</c>.
		/// </exception>
		public static MessagePackSerializer<T> Create<T>( SerializationContext context )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			Contract.Ensures( Contract.Result<MessagePackSerializer<T>>() != null );

			//Func<SerializationContext, SerializerBuilder<T>> builderProvider;
			ISerializerBuilder<T> builder;
#if NETFX_CORE
			builder = new ExpressionTreeSerializerBuilder<T>();
#elif SILVERLIGHT
			builder = new DynamicMethodSerializerBuilder<T>();
#else
			switch ( context.EmitterFlavor )
			{
#if !WINDOWS_PHONE && !NETFX_35
				case EmitterFlavor.ExpressionBased:
				{
					builder = new ExpressionTreeSerializerBuilder<T>();
					break;
				}
#endif // !WINDOWS_PHONE && !NETFX_35
				case EmitterFlavor.FieldBased:
				{
					builder = new AssemblyBuilderSerializerBuilder<T>();
					break;
				}
				case EmitterFlavor.ContextBased:
				{
					builder = new DynamicMethodSerializerBuilder<T>();
					break;
				}
				default:
				{
					if ( !SerializerDebugging.OnTheFlyCodeDomEnabled )
					{
						throw new NotSupportedException(
							String.Format(
								CultureInfo.CurrentCulture,
								"Flavor '{0:G}'({0:D}) is not supported for serializer instance creation.",
								context.EmitterFlavor
							) 
						);
					}

					builder = new CodeDomSerializerBuilder<T>();
					break;
				}
			}
#endif // NETFX_CORE else

			return new AutoMessagePackSerializer<T>( context, builder );
		}

#if !SILVERLIGHT && !NETFX_35
		private static readonly ConcurrentDictionary<Type, Func<SerializationContext, IMessagePackSingleObjectSerializer>> _creatorCache = new ConcurrentDictionary<Type, Func<SerializationContext, IMessagePackSingleObjectSerializer>>();
#else
		private static readonly object _syncRoot = new object();
		private static readonly Dictionary<Type, Func<SerializationContext, IMessagePackSingleObjectSerializer>> _creatorCache = new Dictionary<Type, Func<SerializationContext, IMessagePackSingleObjectSerializer>>();
#endif

		/// <summary>
		///		Creates new <see cref="IMessagePackSerializer"/> instance with <see cref="SerializationContext.Default"/>.
		/// </summary>
		/// <param name="targetType">Target type.</param>
		/// <returns>
		///		New <see cref="IMessagePackSingleObjectSerializer"/> instance to serialize/deserialize the object tree which the top is <paramref name="targetType"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetType"/> is <c>null</c>.
		/// </exception>
		/// <remarks>
		///		To avoid boxing and strongly typed API is prefered, use <see cref="Create{T}()"/> instead when possible.
		/// </remarks>
		public static IMessagePackSingleObjectSerializer Create( Type targetType )
		{
			return Create( targetType, SerializationContext.Default );
		}

		/// <summary>
		///		Creates new <see cref="IMessagePackSerializer"/> instance with specified <see cref="SerializationContext"/>.
		/// </summary>
		/// <param name="targetType">Target type.</param>
		/// <param name="context">
		///		<see cref="SerializationContext"/> to store known/created serializers.
		/// </param>
		/// <returns>
		///		New <see cref="IMessagePackSingleObjectSerializer"/> instance to serialize/deserialize the object tree which the top is <paramref name="targetType"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="targetType"/> is <c>null</c>.
		///		Or, <paramref name="context"/> is <c>null</c>.
		/// </exception>
		/// <remarks>
		///		To avoid boxing and strongly typed API is prefered, use <see cref="Create{T}(SerializationContext)"/> instead when possible.
		/// </remarks>
		public static IMessagePackSingleObjectSerializer Create( Type targetType, SerializationContext context )
		{
			if ( targetType == null )
			{
				throw new ArgumentNullException( "targetType" );
			}

			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			Contract.Ensures( Contract.Result<IMessagePackSerializer>() != null );

			// MPS.Create should always return new instance, and creator delegate should be cached for performance.
#if NETFX_CORE
			var factory =
				_creatorCache.GetOrAdd(
					targetType,
					type =>
					{
						var contextParameter = Expression.Parameter( typeof( SerializationContext ), "context" );
						// Utilize covariance of delegate.
						return
							Expression.Lambda<Func<SerializationContext, IMessagePackSingleObjectSerializer>>(
								Expression.Call(
									null,
									Metadata._MessagePackSerializer.Create1_Method.MakeGenericMethod( type ),
									contextParameter
								),
								contextParameter
							).Compile();
					}
				);
#elif SILVERLIGHT || NETFX_35
			Func<SerializationContext, IMessagePackSingleObjectSerializer> factory;

			lock ( _syncRoot )
			{
				_creatorCache.TryGetValue( targetType, out factory );
			}

			if ( factory == null )
			{
				// Utilize covariance of delegate.
				factory =
					Delegate.CreateDelegate(
						typeof( Func<SerializationContext, IMessagePackSingleObjectSerializer> ),
						Metadata._MessagePackSerializer.Create1_Method.MakeGenericMethod( targetType )
						) as Func<SerializationContext, IMessagePackSingleObjectSerializer>;

				Contract.Assert( factory != null );

				lock ( _syncRoot )
				{
					_creatorCache[ targetType ] = factory;
				}
			}
#else
			var factory =
				_creatorCache.GetOrAdd(
					targetType,
					type =>
						// Utilize covariance of delegate.
						Delegate.CreateDelegate(
							typeof( Func<SerializationContext, IMessagePackSingleObjectSerializer> ),
							Metadata._MessagePackSerializer.Create1_Method.MakeGenericMethod( type )
						) as Func<SerializationContext, IMessagePackSingleObjectSerializer>
				);
#endif
			return factory( context );
		}
	}
}
