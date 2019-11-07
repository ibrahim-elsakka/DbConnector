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

using DbConnector.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace DbConnector.Core
{
    public partial class DbConnector<TDbConnection> : IDbConnector<TDbConnection>
       where TDbConnection : DbConnection
    {
        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>       
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>)> Read<T1, T2>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>()),
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.Default));
                        p.DeferDisposable(odr);

                        bool hasNext = true;
                        int index = 1;

                        while ((hasNext && odr.HasRows) || (odr.NextResult() && (++index) > 0))
                        {
                            if (p.Token.IsCancellationRequested)
                                return d;

                            switch (index)
                            {
                                case 1:
                                    d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                    break;
                                case 2:
                                    d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                    break;
                                default:
                                    return d;
                            }

                            hasNext = odr.NextResult();
                            index++;
                        }


                        return d;
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>    
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> Read<T1, T2, T3>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>()),
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.Default));
                        p.DeferDisposable(odr);

                        bool hasNext = true;
                        int index = 1;

                        while ((hasNext && odr.HasRows) || (odr.NextResult() && (++index) > 0))
                        {
                            if (p.Token.IsCancellationRequested)
                                return d;

                            switch (index)
                            {
                                case 1:
                                    d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                    break;
                                case 2:
                                    d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                    break;
                                case 3:
                                    d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                    break;
                                default:
                                    return d;
                            }

                            hasNext = odr.NextResult();
                            index++;
                        }

                        return d;
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>          
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>)> Read<T1, T2, T3, T4>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>()),
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.Default));
                        p.DeferDisposable(odr);

                        bool hasNext = true;
                        int index = 1;

                        while ((hasNext && odr.HasRows) || (odr.NextResult() && (++index) > 0))
                        {
                            if (p.Token.IsCancellationRequested)
                                return d;

                            switch (index)
                            {
                                case 1:
                                    d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                    break;
                                case 2:
                                    d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                    break;
                                case 3:
                                    d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                    break;
                                case 4:
                                    d.Item4 = p.IsBuffered ? odr.ToList<T4>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T4>(p.Token, p.JobCommand);
                                    break;
                                default:
                                    return d;
                            }

                            hasNext = odr.NextResult();
                            index++;
                        }

                        return d;
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>                 
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>)> Read<T1, T2, T3, T4, T5>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>()),
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.Default));
                        p.DeferDisposable(odr);

                        bool hasNext = true;
                        int index = 1;

                        while ((hasNext && odr.HasRows) || (odr.NextResult() && (++index) > 0))
                        {
                            if (p.Token.IsCancellationRequested)
                                return d;

                            switch (index)
                            {
                                case 1:
                                    d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                    break;
                                case 2:
                                    d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                    break;
                                case 3:
                                    d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                    break;
                                case 4:
                                    d.Item4 = p.IsBuffered ? odr.ToList<T4>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T4>(p.Token, p.JobCommand);
                                    break;
                                case 5:
                                    d.Item5 = p.IsBuffered ? odr.ToList<T5>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T5>(p.Token, p.JobCommand);
                                    break;
                                default:
                                    return d;
                            }

                            hasNext = odr.NextResult();
                            index++;
                        }

                        return d;
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>                        
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>)> Read<T1, T2, T3, T4, T5, T6>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>()),
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.Default));
                        p.DeferDisposable(odr);

                        bool hasNext = true;
                        int index = 1;

                        while ((hasNext && odr.HasRows) || (odr.NextResult() && (++index) > 0))
                        {
                            if (p.Token.IsCancellationRequested)
                                return d;

                            switch (index)
                            {
                                case 1:
                                    d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                    break;
                                case 2:
                                    d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                    break;
                                case 3:
                                    d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                    break;
                                case 4:
                                    d.Item4 = p.IsBuffered ? odr.ToList<T4>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T4>(p.Token, p.JobCommand);
                                    break;
                                case 5:
                                    d.Item5 = p.IsBuffered ? odr.ToList<T5>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T5>(p.Token, p.JobCommand);
                                    break;
                                case 6:
                                    d.Item6 = p.IsBuffered ? odr.ToList<T6>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T6>(p.Token, p.JobCommand);
                                    break;
                                default:
                                    return d;
                            }

                            hasNext = odr.NextResult();
                            index++;
                        }

                        return d;
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}, IEnumerable{T7}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>                        
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}, IEnumerable{T7}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>)> Read<T1, T2, T3, T4, T5, T6, T7>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>(), Enumerable.Empty<T7>()),
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.Default));
                        p.DeferDisposable(odr);

                        bool hasNext = true;
                        int index = 1;

                        while ((hasNext && odr.HasRows) || (odr.NextResult() && (++index) > 0))
                        {
                            if (p.Token.IsCancellationRequested)
                                return d;

                            switch (index)
                            {
                                case 1:
                                    d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                    break;
                                case 2:
                                    d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                    break;
                                case 3:
                                    d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                    break;
                                case 4:
                                    d.Item4 = p.IsBuffered ? odr.ToList<T4>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T4>(p.Token, p.JobCommand);
                                    break;
                                case 5:
                                    d.Item5 = p.IsBuffered ? odr.ToList<T5>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T5>(p.Token, p.JobCommand);
                                    break;
                                case 6:
                                    d.Item6 = p.IsBuffered ? odr.ToList<T6>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T6>(p.Token, p.JobCommand);
                                    break;
                                case 7:
                                    d.Item7 = p.IsBuffered ? odr.ToList<T7>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T7>(p.Token, p.JobCommand);
                                    break;
                                default:
                                    return d;
                            }

                            hasNext = odr.NextResult();
                            index++;
                        }

                        return d;
                    }
                );
        }

        /// <summary>
        ///  <para>Creates a <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}, IEnumerable{T7}, IEnumerable{T8}}}"/> able to execute a reader based on the <paramref name="onInit"/> action.</para>
        ///  <para>Valid <typeparamref name="T"/> types: <see cref="DataSet"/>, <see cref="DataTable"/>, <see cref="Dictionary{string,object}"/>, any .NET built-in type, or any struct or class with a parameterless constructor not assignable from <see cref="IEnumerable"/> (Note: only properties will be mapped).</para>
        ///  See also:
        ///  <seealso cref="DbCommand.ExecuteReader"/>
        /// </summary>
        /// <remarks>
        /// This will use the <see cref="CommandBehavior.Default"/> behavior by default.
        /// </remarks>                        
        /// <param name="onInit">Action that is used to configure the <see cref="IDbJobCommand"/>.</param>
        /// <returns>The <see cref="IDbJob{ValueTuple{IEnumerable{T1}, IEnumerable{T2}, IEnumerable{T3}, IEnumerable{T4}, IEnumerable{T5}, IEnumerable{T6}, IEnumerable{T7}, IEnumerable{T8}}}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when onInit is null.</exception>
        public IDbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>, IEnumerable<T8>)> Read<T1, T2, T3, T4, T5, T6, T7, T8>(Action<IDbJobCommand> onInit)
        {
            if (onInit == null)
            {
                throw new ArgumentNullException("onInit cannot be null!");
            }

            return new DbJob<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>, IEnumerable<T5>, IEnumerable<T6>, IEnumerable<T7>, IEnumerable<T8>), TDbConnection>
                (
                    setting: _jobSetting,
                    state: new DbConnectorState { Flags = _flags, OnInit = onInit },
                    onInit: () => (Enumerable.Empty<T1>(), Enumerable.Empty<T2>(), Enumerable.Empty<T3>(), Enumerable.Empty<T4>(), Enumerable.Empty<T5>(), Enumerable.Empty<T6>(), Enumerable.Empty<T7>(), Enumerable.Empty<T8>()),
                    onCommands: (conn, state) => BuildJobCommand(conn, state),
                    onExecute: (d, p) =>
                    {
                        DbDataReader odr = p.Command.ExecuteReader(ConfigureCommandBehavior(p, CommandBehavior.Default));
                        p.DeferDisposable(odr);

                        bool hasNext = true;
                        int index = 1;

                        while ((hasNext && odr.HasRows) || (odr.NextResult() && (++index) > 0))
                        {
                            if (p.Token.IsCancellationRequested)
                                return d;

                            switch (index)
                            {
                                case 1:
                                    d.Item1 = p.IsBuffered ? odr.ToList<T1>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T1>(p.Token, p.JobCommand);
                                    break;
                                case 2:
                                    d.Item2 = p.IsBuffered ? odr.ToList<T2>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T2>(p.Token, p.JobCommand);
                                    break;
                                case 3:
                                    d.Item3 = p.IsBuffered ? odr.ToList<T3>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T3>(p.Token, p.JobCommand);
                                    break;
                                case 4:
                                    d.Item4 = p.IsBuffered ? odr.ToList<T4>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T4>(p.Token, p.JobCommand);
                                    break;
                                case 5:
                                    d.Item5 = p.IsBuffered ? odr.ToList<T5>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T5>(p.Token, p.JobCommand);
                                    break;
                                case 6:
                                    d.Item6 = p.IsBuffered ? odr.ToList<T6>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T6>(p.Token, p.JobCommand);
                                    break;
                                case 7:
                                    d.Item7 = p.IsBuffered ? odr.ToList<T7>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T7>(p.Token, p.JobCommand);
                                    break;
                                case 8:
                                    d.Item8 = p.IsBuffered ? odr.ToList<T8>(p.Token, p.JobCommand)
                                            : odr.ToEnumerable<T8>(p.Token, p.JobCommand);
                                    break;
                                default:
                                    return d;
                            }

                            hasNext = odr.NextResult();
                            index++;
                        }

                        return d;
                    }
                );
        }
    }
}