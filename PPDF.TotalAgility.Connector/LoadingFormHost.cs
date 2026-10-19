using System;
using System.Threading;
using System.Windows.Forms;

namespace PPDF.TotalAgility.Connector
{
    /// <summary>
    /// Shows the "please wait" LoadingForm on its own STA thread and closes it again.
    ///
    /// <see cref="Start"/> returns only once the form is on screen (or after a short timeout),
    /// so <see cref="Close"/> always has a form to close, even when the API call that follows
    /// fails immediately. <see cref="Close"/> never waits for ever.
    /// </summary>
    internal sealed class LoadingFormHost
    {
        private const int StartTimeoutMs = 5000;
        private const int CloseTimeoutMs = 5000;

        private readonly ManualResetEvent _shown = new ManualResetEvent(false);
        private LoadingForm _form;
        private Thread _thread;

        public void Start()
        {
            _thread = new Thread(Run);
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.IsBackground = true;   // never keeps the host process alive
            _thread.Start();

            _shown.WaitOne(StartTimeoutMs);
        }

        private void Run()
        {
            try
            {
                _form = new LoadingForm();
                _form.Shown += (s, e) => _shown.Set();
                Application.Run(_form);
            }
            catch (Exception ex)
            {
                DebugLog.Write("LoadingFormHost", "Loading form failed: " + ex.Message);
            }
            finally
            {
                _shown.Set();   // also releases Start() if the form could not be shown
            }
        }

        public void Close()
        {
            LoadingForm form = _form;
            try
            {
                if (form != null && form.IsHandleCreated)
                    form.Invoke(new Action(form.Close));
            }
            catch (Exception ex)
            {
                DebugLog.Write("LoadingFormHost", "Closing loading form failed: " + ex.Message);
            }

            if (_thread != null)
                _thread.Join(CloseTimeoutMs);
        }
    }
}
