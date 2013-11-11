#region -- License Terms --
//
// MessagePack for CLI
//
// Copyright (C) 2010-2013 FUJIWARA, Yusuke
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

using MsgPack.Serialization.AbstractSerializers;

namespace MsgPack.Serialization.EmittingSerializers
{
	/// <summary>
	///		An <see cref="ISerializerCodeGenerationContext"/> for <see cref="AssemblyBuilderSerializerBuilder{TObject}"/>.
	/// </summary>
	internal class AssemblyBuilderCodeGenerationContext : ISerializerCodeGenerationContext
	{
		public Version Version { get; set; }

		private readonly SerializationContext _context;
		private readonly SerializationMethodGeneratorManager _generatorManager;

		public AssemblyBuilderCodeGenerationContext( SerializationContext context, SerializationMethodGeneratorManager generatorManager )
		{
			this._context = context;
			this._generatorManager = generatorManager;
		}

		/// <summary>
		///		Create new <see cref="AssemblyBuilderEmittingContext"/> for specified <see cref="Type"/>.
		/// </summary>
		/// <param name="type">The type will be emitted.</param>
		/// <returns><see cref="AssemblyBuilderEmittingContext"/>.</returns>
		public AssemblyBuilderEmittingContext CreateEmittingContext( Type type )
		{
			return new AssemblyBuilderEmittingContext( this._context, type, this._generatorManager.CreateEmitter( type, EmitterFlavor.FieldBased ) );
		}

		/// <summary>
		///		Determines that whether built-in serializer for specified type exists or not.
		/// </summary>
		/// <param name="type">The type for check.</param>
		/// <returns>
		///   <c>true</c> if built-in serializer for specified type exists; <c>false</c>, otherwise.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public bool BuiltInSerializerExists( Type type )
		{
			if( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			return type.IsArray || SerializerRepository.Default.Contains( type );
		}
	}
}