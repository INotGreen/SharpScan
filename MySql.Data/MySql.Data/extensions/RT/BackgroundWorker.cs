// Copyright © 2004, 2013, Oracle and/or its affiliates. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using System.ComponentModel;

namespace MySql.Data.MySqlClient
{
    /// <summary>
    /// An implementation of the missing System.ComponentModel.BackgroundWorker in RT.
    /// </summary>
    public class BackgroundWorker : DependencyObject
    {
        public BackgroundWorker()
        {            
        }

        public void CancelAsync()
        {
            if (!WorkerSupportsCancellation)
                throw new NotSupportedException();
            CancellationPending = true;
        }

        public bool CancellationPending { get; private set; }

        public event ProgressChangedEventHandler ProgressChanged;

        public void ReportProgress(int percentProgress)
        {
            ReportProgress(percentProgress, null);
        }

        public void ReportProgress(int percentProgress, object userState)
        {
            if (ProgressChanged != null)
                base.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                  () =>
                  {
                      ProgressChanged(this, new ProgressChangedEventArgs(percentProgress, userState));
                  });
        }

        public bool WorkerReportsProgress { get; set; }
        public bool WorkerSupportsCancellation { get; set; }
        public bool IsBusy { get; set; }

        public event DoWorkEventHandler DoWork;
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;
        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            if (RunWorkerCompleted != null)
                RunWorkerCompleted(this, e);
        }

        public void RunWorkerAsync()
        {
            RunWorkerAsync(null);
        }

        public async void RunWorkerAsync(object userState)
        {
            if (DoWork != null)
            {
                CancellationPending = false;
                IsBusy = true;
                try
                {
                    var args = new DoWorkEventArgs { Argument = userState };
                    await Task.Run(() =>
                    {
                        DoWork(this, args);
                    });
                    IsBusy = false;
                    OnRunWorkerCompleted(new RunWorkerCompletedEventArgs { Result = args.Result });
                }
                catch (Exception ex)
                {
                    IsBusy = false;
                    OnRunWorkerCompleted(new RunWorkerCompletedEventArgs { Error = ex });
                }
            }
        }
    }

    public delegate void DoWorkEventHandler(object sender, DoWorkEventArgs e);

    public class DoWorkEventArgs : EventArgs
    {
        public DoWorkEventArgs()
        { }

        public DoWorkEventArgs(object argument)
        {
            Argument = argument;
        }

        public object Argument { get; set; }
        public bool Cancel { get; set; }
        public object Result { get; set; }
    }

    public delegate void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);

    public class RunWorkerCompletedEventArgs : EventArgs
    {
        public RunWorkerCompletedEventArgs()
        { }

        public RunWorkerCompletedEventArgs(object result, Exception error, bool cancelled)
        {
            Result = result;
            Error = error;
            Cancelled = cancelled;
        }

        public Exception Error { get; set; }
        public object Result { get; set; }
        public bool Cancelled { get; set; }
    }
}