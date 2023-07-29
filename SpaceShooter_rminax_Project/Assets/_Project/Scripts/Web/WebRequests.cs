using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace _Project.Scripts.Web
{
    public static class WebRequests
    {
        private class WebRequestsMonoBehavior : MonoBehaviour{}

        private static WebRequestsMonoBehavior _webRequestsMonoBehaviour;

        private static void Init()
        {
            if (_webRequestsMonoBehaviour != null) return;

            var gameObject = new GameObject("WebRequests");
            _webRequestsMonoBehaviour = gameObject.AddComponent<WebRequestsMonoBehavior>();
        }


        public static void Get(string url, Action<UnityWebRequest> setHeaderAction, Action<string> onError,
            Action<string> onSuccess)
        {
            Init();
            _webRequestsMonoBehaviour.StartCoroutine(GetCoroutine(url, setHeaderAction, onError, onSuccess));
        }

        public static void Get(string url, Action<string> onError, Action<string> onSuccess)
        {
            Init();
            _webRequestsMonoBehaviour.StartCoroutine(GetCoroutine(url, null, onError, onSuccess));
        }

        private static IEnumerator GetCoroutine(string url, Action<UnityWebRequest> setHeaderAction,
            Action<string> onError, Action<string> onSuccess)
        {
            using var unityWebRequest = UnityWebRequest.Get(url);

            setHeaderAction?.Invoke(unityWebRequest);

            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result
                is UnityWebRequest.Result.ConnectionError
                or UnityWebRequest.Result.ProtocolError
                or UnityWebRequest.Result.DataProcessingError)
            {
                onError?.Invoke(unityWebRequest.error);
                yield break;
            }

            onSuccess?.Invoke(unityWebRequest.downloadHandler.text);
        }

        public static void Post(string url, Dictionary<string, string> formFields, Action<string> onError,
            Action<string> onSuccess)
        {
            Init();
            _webRequestsMonoBehaviour.StartCoroutine(GetCoroutinePost(url, formFields, onError, onSuccess));
        }

        public static void Post(string url, string postData, Action<string> onError, Action<string> onSuccess)
        {
            Init();
            _webRequestsMonoBehaviour.StartCoroutine(GetCoroutinePost(url, postData, onError, onSuccess));
        }

        public static void PostJson(string url, string jsonData, Action<string> onError, Action<string> onSuccess)
        {
            Init();
            _webRequestsMonoBehaviour.StartCoroutine(GetCoroutinePostJson(url, null, jsonData, onError, onSuccess));
        }

        public static void PostJson(string url, Action<UnityWebRequest> setHeaderAction, string jsonData,
            Action<string> onError, Action<string> onSuccess)
        {
            Init();
            _webRequestsMonoBehaviour.StartCoroutine(GetCoroutinePostJson(url, setHeaderAction, jsonData, onError,
                onSuccess));
        }

        private static IEnumerator GetCoroutinePost(string url, Dictionary<string, string> formFields,
            Action<string> onError, Action<string> onSuccess)
        {
            using var unityWebRequest = UnityWebRequest.Post(url, formFields);
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result
                is UnityWebRequest.Result.ConnectionError
                or UnityWebRequest.Result.DataProcessingError
                or UnityWebRequest.Result.ProtocolError)
            {
                // Error
                onError(unityWebRequest.error);
            }
            else
            {
                onSuccess(unityWebRequest.downloadHandler.text);
            }
        }

        private static IEnumerator GetCoroutinePost(string url, string postData, Action<string> onError,
            Action<string> onSuccess)
        {
            using var unityWebRequest = UnityWebRequest.PostWwwForm(url, postData);

            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result
                is UnityWebRequest.Result.ConnectionError
                or UnityWebRequest.Result.DataProcessingError
                or UnityWebRequest.Result.ProtocolError)
            {
                // Error
                onError(unityWebRequest.error);
            }
            else
            {
                onSuccess(unityWebRequest.downloadHandler.text);
            }
        }

        private static IEnumerator GetCoroutinePostJson(string url, Action<UnityWebRequest> setHeaderAction,
            string jsonData, Action<string> onError, Action<string> onSuccess)
        {
            using var unityWebRequest = new UnityWebRequest(url, "POST");

            var bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            unityWebRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");

            setHeaderAction?.Invoke(unityWebRequest);

            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError ||
                unityWebRequest.result == UnityWebRequest.Result.DataProcessingError ||
                unityWebRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // Error
                onError(unityWebRequest.error);
            }
            else
            {
                onSuccess(unityWebRequest.downloadHandler.text);
            }
        }

        public static void Put(string url, string bodyData, Action<string> onError, Action<string> onSuccess)
        {
            Init();
            _webRequestsMonoBehaviour.StartCoroutine(GetCoroutinePut(url, bodyData, onError, onSuccess));
        }

        private static IEnumerator GetCoroutinePut(string url, string bodyData, Action<string> onError,
            Action<string> onSuccess)
        {
            using var unityWebRequest = UnityWebRequest.Put(url, bodyData);
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result
                is UnityWebRequest.Result.ConnectionError
                or UnityWebRequest.Result.DataProcessingError
                or UnityWebRequest.Result.ProtocolError)
            {
                // Error
                onError(unityWebRequest.error);
            }
            else
            {
                onSuccess(unityWebRequest.downloadHandler.text);
            }
        }

        public static void GetTexture(string url, Action<string> onError, Action<Texture2D> onSuccess)
        {
            Init();
            _webRequestsMonoBehaviour.StartCoroutine(GetTextureCoroutine(url, onError, onSuccess));
        }

        private static IEnumerator GetTextureCoroutine(string url, Action<string> onError, Action<Texture2D> onSuccess)
        {
            using var unityWebRequest = UnityWebRequestTexture.GetTexture(url);

            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result
                is UnityWebRequest.Result.ConnectionError
                or UnityWebRequest.Result.DataProcessingError
                or UnityWebRequest.Result.ProtocolError)
            {
                // Error
                onError(unityWebRequest.error);
            }
            else
            {
                if (unityWebRequest.downloadHandler is DownloadHandlerTexture downloadHandlerTexture)
                    onSuccess(downloadHandlerTexture.texture);
            }
        }
    }
}