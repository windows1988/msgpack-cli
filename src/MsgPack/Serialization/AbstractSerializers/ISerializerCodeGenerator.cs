﻿#region -- License Terms --
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

namespace MsgPack.Serialization.AbstractSerializers
{
	/// <summary>
	///		Defines a common interface for serializer builder which supports code generation.
	/// </summary>
	internal interface ISerializerCodeGenerator
	{
		/// <summary>
		///		Builds the serializer code using specified code generation context.
		/// </summary>
		/// <param name="context">
		///		The <see cref="ISerializerCodeGenerationContext"/> which holds configuration and stores generated code constructs.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="context"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="NotSupportedException">
		///		This class does not support code generation.
		/// </exception>
		void BuildSerializerCode( ISerializerCodeGenerationContext context );

		/// <summary>
		///		Creates new <see cref="ISerializerCodeGenerationContext"/> instance dedicated to this builder.
		/// </summary>
		/// <param name="context">The <see cref="SerializationContext"/>.</param>
		/// <returns>The new <see cref="ISerializerCodeGenerationContext"/> instance dedicated to this builder.</returns>
		/// <exception cref="ArgumentNullException">
		///		<paramref name="context"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///		A type of <paramref name="context"/> is not valid.
		/// </exception>
		/// <exception cref="NotSupportedException">
		///		This class does not support code generation.
		/// </exception>
		ISerializerCodeGenerationContext CreateGenerationContextForCodeGeneration( SerializationContext context );
	}
}