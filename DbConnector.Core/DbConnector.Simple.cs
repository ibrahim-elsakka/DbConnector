﻿//Copyright 2019 Robert Orama

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace DbConnector.Core
{
    public partial class DbConnector<TDbConnection> : IDbConnector<TDbConnection>
       where TDbConnection : DbConnection
    {
        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{T}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{T}}"/>.</returns>
        public IDbJob<IEnumerable<T>> Read<T>(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<IEnumerable<T>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteRead(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<T>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{T}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{IEnumerable{T}}"/>.</returns>
        public IDbJob<IEnumerable<T>> Read<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<IEnumerable<T>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteRead(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<T>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param>        
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<T> ReadFirst<T>(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadFirst(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> and <see cref="CommandBehavior.SingleRow"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        public IDbJob<T> ReadFirst<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadFirst(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param>        
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        public IDbJob<T> ReadFirstOrDefault<T>(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only the first row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        public IDbJob<T> ReadFirstOrDefault<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadFirstOrDefault(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param>        
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<T> ReadSingle<T>(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadSingle(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result is empty.</exception>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<T> ReadSingle<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadSingle(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param>        
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<T> ReadSingleOrDefault<T>(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadSingleOrDefault(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Use this to load only a single row from the query into a result of <typeparamref name="T"/>.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidOperationException">The query result has more than one result.</exception>
        public IDbJob<T> ReadSingleOrDefault<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<T, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadSingleOrDefault(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{T}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{T}}"/>.</returns>
        public IDbJob<List<T>> ReadToList<T>(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<List<T>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToList(d, p)
                ).SetOnError((d, e) => new List<T>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{T}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="System.Collections.IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <typeparam name="T">The element type to use for the single result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{IEnumerable{T}}"/>.</returns>
        public IDbJob<List<T>> ReadToList<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<List<T>, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadToList(d, p)
                ).SetOnError((d, e) => new List<T>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{System.Data.DataTable}"/> able to execute a reader based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{System.Data.DataTable}"/>.</returns>
        public IDbJob<DataTable> ReadToDataTable(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<DataTable, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToDataTable(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{System.Data.DataTable}"/> able to execute a reader based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>        
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{System.Data.DataTable}"/>.</returns>
        public IDbJob<DataTable> ReadToDataTable(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<DataTable, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadToDataTable(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{System.Data.DataSet}"/> able to execute a reader based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{System.Data.DataSet}"/>.</returns>
        public IDbJob<DataSet> ReadToDataSet(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<DataSet, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToDataSet(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{System.Data.DataSet}"/> able to execute a reader based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{System.Data.DataSet}"/>.</returns>
        public IDbJob<DataSet> ReadToDataSet(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<DataSet, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadToDataSet(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{List{KeyValuePair{string, object}}}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when requiring a generic data list from the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{List{KeyValuePair{string, object}}}}"/>.</returns>
        public IDbJob<IEnumerable<List<KeyValuePair<string, object>>>> ReadToKeyValuePairs(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<IEnumerable<List<KeyValuePair<string, object>>>, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToKeyValuePairs(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<List<KeyValuePair<string, object>>>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{List{KeyValuePair{string, object}}}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when requiring a generic data list from the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>       
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{IEnumerable{List{KeyValuePair{string, object}}}}"/>.</returns>
        public IDbJob<IEnumerable<List<KeyValuePair<string, object>>>> ReadToKeyValuePairs(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<IEnumerable<List<KeyValuePair<string, object>>>, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadToKeyValuePairs(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<List<KeyValuePair<string, object>>>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{Dictionary{string, object}}}"/> able to execute a reader based on the configured parameters.
        ///  This is usefull when requiring a non-concrete data list from unique columns of the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IEnumerable{Dictionary{string, object}}}"/>.</returns>
        public IDbJob<IEnumerable<Dictionary<string, object>>> ReadToDictionaries(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<IEnumerable<Dictionary<string, object>>, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToDictionaries(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<Dictionary<string, object>>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IEnumerable{Dictionary{string, object}}}"/> able to execute a reader based on the configured parameters.
        ///  This is usefull when requiring a non-concrete data list from unique columns of the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{IEnumerable{Dictionary{string, object}}}"/>.</returns>
        public IDbJob<IEnumerable<Dictionary<string, object>>> ReadToDictionaries(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<IEnumerable<Dictionary<string, object>>, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadToDictionaries(d, p)
                ).SetOnError((d, e) => Enumerable.Empty<Dictionary<string, object>>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{List{KeyValuePair{string, object}}}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when requiring a generic data list from the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{List{List{KeyValuePair{string, object}}}}"/>.</returns>
        public IDbJob<List<List<KeyValuePair<string, object>>>> ReadToListOfKeyValuePairs(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<List<List<KeyValuePair<string, object>>>, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToListOfKeyValuePairs(d, p)
                ).SetOnError((d, e) => new List<List<KeyValuePair<string, object>>>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{List{KeyValuePair{string, object}}}}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when requiring a generic data list from the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{List{List{KeyValuePair{string, object}}}}"/>.</returns>
        public IDbJob<List<List<KeyValuePair<string, object>>>> ReadToListOfKeyValuePairs(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<List<List<KeyValuePair<string, object>>>, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadToListOfKeyValuePairs(d, p)
                ).SetOnError((d, e) => new List<List<KeyValuePair<string, object>>>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{Dictionary{string, object}}}"/> able to execute a reader based on the configured parameters.
        ///  This is usefull when requiring a non-concrete data list from unique columns of the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{List{Dictionary{string, object}}}"/>.</returns>
        public IDbJob<List<Dictionary<string, object>>> ReadToListOfDictionaries(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<List<Dictionary<string, object>>, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToListOfDictionaries(d, p)
                ).SetOnError((d, e) => new List<Dictionary<string, object>>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{List{Dictionary{string, object}}}"/> able to execute a reader based on the configured parameters.
        ///  This is usefull when requiring a non-concrete data list from unique columns of the query result.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.SingleResult"/> behavior by default.
        /// </remarks>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{List{Dictionary{string, object}}}"/>.</returns>
        public IDbJob<List<Dictionary<string, object>>> ReadToListOfDictionaries(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<List<Dictionary<string, object>>, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadToListOfDictionaries(d, p)
                ).SetOnError((d, e) => new List<Dictionary<string, object>>());
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IDbCollectionSet}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when wanting to create a concrete object from multiple/different queries.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{IDbCollectionSet}"/>.</returns>
        public IDbJob<IDbCollectionSet> ReadToDbCollectionSet(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<IDbCollectionSet, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => new DbCollectionSet(),
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteReadToDbCollectionSet(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{IDbCollectionSet}"/> able to execute a reader based on the configured parameters.</para>
        ///  <para>This is usefull when wanting to create a concrete object from multiple/different queries.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{IDbCollectionSet}"/>.</returns>
        public IDbJob<IDbCollectionSet> ReadToDbCollectionSet(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<IDbCollectionSet, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onInit: () => new DbCollectionSet(),
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteReadToDbCollectionSet(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> to get the first column of the first row in the result
        ///  set returned by the query. All other columns and rows are ignored.</para>
        ///  <para>Valid <typeparamref name="T"/> types: any .NET built-in type, or any non-reference type that is not assignable from <see cref="System.Collections.IEnumerable"/> or <see cref="IListSource"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidCastException">Thrown when <typeparamref name="T"/> is not supported.</exception>
        public IDbJob<T> Scalar<T>(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            Type tType = typeof(T);

            if (
                   !DbConnectorUtilities._directTypeMap.Contains(tType)
                && !(tType.IsValueType && !(typeof(IEnumerable).IsAssignableFrom(tType) || typeof(IListSource).IsAssignableFrom(tType)))
                && !tType.IsArray
             )
            {
                throw new InvalidCastException("The type " + tType + " is not supported.");
            }

            return new DbJob<T, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteScalar(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> to get the first column of the first row in the result
        ///  set returned by the query. All other columns and rows are ignored.</para>
        ///  <para>Valid <typeparamref name="T"/> types: any .NET built-in type, or any non-reference type that is not assignable from <see cref="System.Collections.IEnumerable"/> or <see cref="IListSource"/>.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteScalar"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        /// <exception cref="InvalidCastException">Thrown when <typeparamref name="T"/> is not supported.</exception>
        public IDbJob<T> Scalar<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            Type tType = typeof(T);

            if (
                   !DbConnectorUtilities._directTypeMap.Contains(tType)
                && !(tType.IsValueType && !(typeof(IEnumerable).IsAssignableFrom(tType) || typeof(IListSource).IsAssignableFrom(tType)))
                && !tType.IsArray
             )
            {
                throw new InvalidCastException("The type " + tType + " is not supported.");
            }

            return new DbJob<T, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteScalar(d, p)
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{int?}"/> able to execute a non-query based on the configured parameters.</para>
        ///  <para> The result will be null if the non-query fails. Otherwise, the result will be the number of rows affected if the non-query ran successfully.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{int?}"/>.</returns>
        public IDbJob<int?> NonQuery(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<int?, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteNonQuery(d, p)
                )
                .SetOnError((d, e) => null)
                .SetWithIsolationLevel(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{int?}"/> able to execute a non-query based on the configured parameters.</para>
        ///  <para> The result will be null if the non-query fails. Otherwise, the result will be the number of rows affected if the non-query ran successfully.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{int?}"/>.</returns>
        public IDbJob<int?> NonQuery(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<int?, TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteNonQuery(d, p)
                )
                .SetOnError((d, e) => null)
                .SetWithIsolationLevel(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a non-query based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param> 
        /// <param name="mapSettings">The <see cref="ColumnMapSetting"/> to use.</param> 
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param> 
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> to use. (Optional)</param> 
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. (Optional)</param> 
        /// <param name="flags">The flags to use. (Optional)</param> 
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        public IDbJob<T> NonQuery<T>(
            string sql,
            ColumnMapSetting mapSettings,
            object param = null,
            CommandType commandType = CommandType.Text,
            CommandBehavior? commandBehavior = null,
            int? commandTimeout = null,
            DbJobCommandFlags flags = DbJobCommandFlags.None)
        {
            return new DbJob<T, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, mapSettings, param, commandType, commandBehavior, commandTimeout, flags),
                    onExecute: (d, p) => OnExecuteNonQuery(d, p)
                )
                .SetWithIsolationLevel(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{T}"/> able to execute a non-query based on the configured parameters.</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <typeparam name="T">The element type to use for the result.</typeparam>
        /// <param name="sql">The query text command to run against the data source.</param>
        /// <param name="param">The parameter to use. <see cref="DbJobParameterCollection.AddFor(object, bool, string, string)"/> restrictions apply. (Optional)</param> 
        /// <param name="commandType">The <see cref="CommandType"/> to use. (Optional)</param>
        /// <returns>The <see cref="IDbJob{T}"/>.</returns>
        public IDbJob<T> NonQuery<T>(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text)
        {
            return new DbJob<T, TDbConnection>
               (
                    setting: _jobSetting,
                    state: new DbConnectorSimpleState { Flags = _flags },
                    onCommands: (conn, state) => BuildJobCommandForSimpleState(conn, state, sql, param, commandType),
                    onExecute: (d, p) => OnExecuteNonQuery(d, p)
                )
                .SetWithIsolationLevel(IsolationLevel.ReadCommitted);
        }
    }
}
