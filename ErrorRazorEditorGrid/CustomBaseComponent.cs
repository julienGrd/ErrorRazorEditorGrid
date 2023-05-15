
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ErrorRazorEditorGrid
{
    public class CustomBaseComponent : ComponentBase, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isPreRender = true;

        protected void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        //[Inject]
        //protected Locator Locator { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            //Console.WriteLine($"-----------------------{this.GetType().Name} OnAfterRender: " + DateTime.Now.ToString("HH:mm:ss:fff"));
            base.OnAfterRender(firstRender);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            //await this.InvokeJsAsync("makeTooltip");
            _isPreRender = false;
            await base.OnAfterRenderAsync(firstRender);
        }

        protected async Task MakeTooltip(string pSelector)
        {
            await this.InvokeJsAsync("makeTooltip", pSelector);
        }

        protected async Task DestroyTooltip(string pSelector)
        {
            await this.InvokeJsAsync("destroyTooltip", pSelector);
        }

        protected async Task HideTooltip()
        {
            await this.InvokeJsAsync("hideTooltip");
        }


        protected IJSRuntime GetJSRuntime()
        {
            return this.JsRuntime;
        }

        //[Inject]
        //protected Sotsera.Blazor.Toaster.IToaster Toaster { get; set; }

        [Inject]
        protected virtual ILogger<CustomBaseComponent> Logger { get; set; }



        protected void ShowError(string pMsg, Exception pException, object data = null)
        {
            LogException(pMsg, pException, data);
            //MsgBoxViewModel.ShowErreur(Locator.Locator_Com, pMsg, pException, null);
            //Toaster.Error(pMsg);
        }

        protected void ShowWarning(string pMsg)
        {
            //MsgBoxViewModel.ShowWarning(Locator.Locator_Com, pMsg);
            //Toaster.Error(pMsg);
        }

        protected void LogException(string pMsg, Exception pException, object data = null)
        {
            //je prefere ne pas inclure les taskCancelled Exception, de toute facon on n'a pas le detail de l'erreur
            if (pException is not TaskCanceledException)
            {
                //this.Locator.AppVM.WriteLigLog($"{pMsg}. ERREUR : {pException}.";
            }
        }

        protected void ShowInfo(string pTitle, string pMsg, Action pAction = null)
        {
            //MsgBoxViewModel.ShowOk(Locator.Locator_Com, pTitle, pMsg, pAction);
            //Toaster.Error(pMsg);
        }

        protected virtual void LaunchStateHasChanged(
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {

            this.InvokeAsync(() =>

            {
                try
                {
                    //Console.WriteLine("---------LaunchStateHasChanged-------------");
                    //Console.WriteLine("member name: " + memberName);
                    //Console.WriteLine("source file path: " + sourceFilePath);
                    //Console.WriteLine("source line number: " + sourceLineNumber);
                    this.StateHasChanged();
                }
                catch (Exception ex)
                {
                    //to prevent app crash when bug appear
                    this.ShowError("Impossible de mettre à jour le composant. Veuillez contacter votre administrateur.", ex);
                }
            });
        }

        public virtual async Task LaunchStateHasChangedAsync(
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {

            await this.InvokeAsync(() =>

            {
                try
                {
                    //Console.WriteLine("---------LaunchStateHasChanged-------------");
                    //Console.WriteLine("member name: " + memberName);
                    //Console.WriteLine("source file path: " + sourceFilePath);
                    //Console.WriteLine("source line number: " + sourceLineNumber);
                    this.StateHasChanged();
                }
                catch (Exception ex)
                {
                    //to prevent app crash when bug appear
                    this.ShowError("Impossible de mettre à jour le composant. Veuillez contacter votre administrateur.", ex);
                }
            });
        }

        protected async Task InvokeJsAsync(string identifier, params object[] args)
        {
            await this.InvokeJsAsync(identifier, false, args);
        }

        protected async Task InvokeJsAsync(string identifier, bool trow, params object[] args)
        {
            await this.InvokeJsAsync<object>(identifier, trow, args);
        }

        protected async Task<TValue> InvokeJsAsync<TValue>(string identifier, params object[] args)
        {
            return await InvokeJsAsync<TValue>(identifier, false, args);
        }

        protected async Task<TValue> InvokeJsAsync<TValue>(string identifier, bool trow, params object[] args)
        {
            try
            {
                return await this.JsRuntime.InvokeAsync<TValue>(identifier, args);

            }
            catch (Exception)
            {
                if (trow)
                {
                    throw;
                }
                else
                {
                    
                    //this.ShowError("Erreur JS. Veuillez contacter votre administrateur.", ex);
                    //this.Logger.LogException(ex);
                    //on fait un truc un peut bizarre mais sinon par moement ca ,e passe pas
                    //etonnament on as bien toute les infos, la stack trace etc

                    return await Task.FromResult<TValue>(default(TValue));
                }
               
            }
        }

        protected async Task<T> InvokeAndRefreshAsync<T>(Task<T> t)
        {
            T value = await t;
            await this.LaunchStateHasChangedAsync();
            return value;
        }
        protected async Task InvokeAndRefreshAsync(Task t)
        {
            await t;
            await this.LaunchStateHasChangedAsync();
        }
        protected void InvokeAndRefresh(Action t)
        {
            t?.Invoke();
            this.LaunchStateHasChanged();
        }

        public virtual void Dispose()
        {
            //if(!_isPreRender)
            //    this.InvokeJsAsync("hideTooltip");
        }
    }
}
