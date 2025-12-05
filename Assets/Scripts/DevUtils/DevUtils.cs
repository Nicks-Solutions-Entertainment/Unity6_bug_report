using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Reflection;
using System.Globalization;
using System.Security;





#if UNITY_EDITOR
using UnityEditor;
#endif

public static class WaitFor
{
    private static readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
    private static readonly WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();

    public static WaitForEndOfFrame EndOfFrame() => endOfFrame;

    public static WaitForFixedUpdate FixedUpdate() => fixedUpdate;


    // Cache de WaitForSeconds para tempos específicos
    private static readonly System.Collections.Generic.Dictionary<float, WaitForSeconds> secondsCache =
        new System.Collections.Generic.Dictionary<float, WaitForSeconds>();
    // Cache de WaitForSeconds para tempos específicos
    private static readonly System.Collections.Generic.Dictionary<int, WaitForFixedUpdates> fUpdatesCache =
        new System.Collections.Generic.Dictionary<int, WaitForFixedUpdates>();

    public static WaitForFixedUpdates FixedUpdates(int ammount)
    {

        if (ammount <= 1)
            ammount = 1;

        if (!fUpdatesCache.TryGetValue(ammount, out var wait))
        {
            wait = new WaitForFixedUpdates(ammount);
            fUpdatesCache[ammount] = wait;
        }
        return wait;
    }

    public static WaitForSeconds Seconds(float seconds)
    {
        if (!secondsCache.TryGetValue(seconds, out var wait))
        {
            wait = new WaitForSeconds(seconds);
            secondsCache[seconds] = wait;
        }
        return wait;
    }

    private static readonly System.Collections.Generic.Dictionary<float, WaitForSecondsRealtime> secondsRTCache =
        new System.Collections.Generic.Dictionary<float, WaitForSecondsRealtime>();

    public static WaitForSecondsRealtime SecondsRealtime(float seconds)
    {
        if (!secondsRTCache.TryGetValue(seconds, out var wait))
        {
            wait = new WaitForSecondsRealtime(seconds);
            secondsRTCache[seconds] = wait;
        }
        return wait;
    }
}

#region Custom YieldInstructions


//public class WaitForUpdates : YieldInstruction
//{
//    private int remainingUpdates;

//    public WaitForUpdates(int updateCount)
//    {
//        remainingUpdates = updateCount;
//    }

//    public bool KeepWaiting
//    {
//        get
//        {
//            if (remainingUpdates > 0)
//            {
//                remainingUpdates--;
//                return true;
//            }
//            return false;
//        }
//    }
//}
public class WaitForFixedUpdates : CustomYieldInstruction
{
    private int remainingFixedUpdates;

    public WaitForFixedUpdates(int fixedUpdateCount)
    {
        remainingFixedUpdates = fixedUpdateCount;
        CoroutineHolder.Instance.StartCoroutine(TrackLoop());
    }

    private IEnumerator TrackLoop()
    {
        while (remainingFixedUpdates > 0)
        {
            yield return WaitFor.FixedUpdate();
            remainingFixedUpdates--;
        }
    }

    public override bool keepWaiting => remainingFixedUpdates > 0;
}

#endregion
public static class DevUtils
{

    public static Mesh DefaultMesh(DefaultMeshType meshType, bool inversed = false)
    {
        Mesh mesh = new Mesh();
        switch (meshType)
        {
            case DefaultMeshType.Cube:
                mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                break;
            case DefaultMeshType.Sphere:
                mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                break;
            case DefaultMeshType.Cylinder:
                mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
                break;
            case DefaultMeshType.Capsule:
                mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
                break;
            case DefaultMeshType.Plane:
                mesh = Resources.GetBuiltinResource<Mesh>("Plane.fbx");
                break;
            case DefaultMeshType.Quad:
                mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
                break;
            default:
                mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                break;


        }
        if (inversed && meshType == DefaultMeshType.Sphere)
        {
            Mesh _iSphare = Resources.Load<Mesh>("InversedSphere");
            if (_iSphare != null)
                mesh = _iSphare;
        }

        return mesh;
    }

    public static Mesh InvertMeshNormals(this Mesh mesh)
    {
        var _normals = mesh.normals;
        for (int i = 0; i < _normals.Length; i++)
        {
            _normals[i] = -_normals[i];
        }
        mesh.normals = _normals;

        var _tris = mesh.triangles;
        for (int i = 0; i < _tris.Length; i += 3)
        {
            int t = _tris[i];
            _tris[i] = _tris[i + 2];
            _tris[i + 2] = t;
        }
        return mesh;
    }

    public static async Task<bool> WaitForEndOfFrame()
    {
        var currentFrame = Time.frameCount;

        while (currentFrame == Time.frameCount)
            await Task.Yield();

        return true;
    }

    public static async Task<bool> WaitForSeconds(float time)
    {
        var t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            await WaitForEndOfFrame();
        }

        return true;
    }

    public static bool IsPrefabMode(this GameObject go)
    {
        string path = go.scene.path;
        return (string.IsNullOrWhiteSpace(path) && !Application.isPlaying);
    }

    [SecuritySafeCritical]
    public static bool TryGetComponentInParent<TComponent>(this GameObject go, out TComponent component)
        where TComponent : Component
    {
        component = go.GetComponentInParent<TComponent>();
        return component != null;
    }

    public enum DefaultMeshType
    {
        Cube, Sphere, Cylinder, Capsule, Plane, Quad
    }

    public static string Bytest2String(this byte[] bytes)
    {
        if (bytes == null) return "NULL";
        return $"[{string.Join(' ', bytes.Select(b => (int)b))}]";
    }
    public static bool MatchBytes(this byte[] array1, byte[] array2, bool DEBUG = false)
    {
        if (array1 == null || array2 == null) return false;
        string dbg_arr1 = array1.Bytest2String();
        string dbg_arr2 = array2.Bytest2String();


        bool matchBytes = true;

        if (array1.Length != array2.Length)
        {
            matchBytes = false;
            
        }
        string b_dbg = "[";
        for (int i = 0; i < array1.Length; i++)
        {
            if (!matchBytes)
            {
                //Debug.Log($"MatchBytes value unmatch: \n arr1 [{string.Join(' ', array1.Select(b => (int)b))}] | arr2 [{string.Join(' ', array2.Select(b => (int)b))}]");
                break;
            }
            bool _ = (int)array1[i] == (int)array2[i];
            if (_)
                b_dbg += $"<color=green>";
            else
                b_dbg += $"<color=red>";
            b_dbg += $"{(int)array1[i]} </color>";
            matchBytes &= _;
        }
        b_dbg += "]";

        if (DEBUG)
            Debug.Log($"<color={(matchBytes ? "green" : "red")}>MatchBytes</color>:  {b_dbg}\n arr1 {b_dbg} | arr2 {dbg_arr2}");
        return matchBytes;
        //return StructuralComparisons.StructuralEqualityComparer.Equals(array1, array2);
    }

    /// <summary>
    /// Execute some funcion after a passed time
    /// </summary>
    /// <param name="action">Action to do after time pass</param>
    public static void WaitAndDo(float TimeInSeconds, Action action, bool condition = true)
    {
        IE_WaitAndDO(TimeInSeconds, action, condition).Run();
    }

    public static async void WaitAndDo(this MonoBehaviour caster, float TimeInSeconds, Action action, bool condition = true)
    {
#if !UNITY_WEBGL || UNITY_EDITOR

        await Task.Delay((int)(TimeInSeconds * 1000));
        if (condition) action?.Invoke();
#elif UNITY_WEBGL && !UNITY_EDITOR
        caster.StartCoroutine(IE_WaitAndDO(TimeInSeconds, action, condition));
#endif
    }

    static IEnumerator IE_WaitAndDO(float TimeInSeconds, Action action, bool condition)
    {
        yield return new WaitForSeconds(TimeInSeconds);
        if (condition) action?.Invoke();
    }


    /// <summary>
    /// Executa uma acao repentinamente enquanto a condicao estabelecia retornar true
    /// </summary>
    /// <param name="repeatInterval"></param>
    /// <param name="action"></param>
    /// <param name="condition"></param>
    public static async void DoWhile(float repeatInterval, bool condition = true, Action action = null)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        while (condition)
        {

            if (condition) action?.Invoke();
            await Task.Delay((int)(repeatInterval * 1000));
        }
#elif UNITY_WEBGL && !UNITY_EDITOR
    IE_DoWhile(repeatInterval,condition,action).Run();
#endif
    }

    static IEnumerator IE_DoWhile(float repeatInterval, bool condition = true, Action action = null)
    {
        while (condition)
        {

            if (condition) action?.Invoke();
            if (repeatInterval > 0)
                yield return new WaitForSeconds((int)(repeatInterval * 1000));
            else
                yield return new WaitForEndOfFrame();
        }
    }


    /// <summary>
    /// Verifica se uma LayerMask contém uma Layer específica.
    /// </summary>
    /// <param name="layerMask">O LayerMask a ser verificado.</param>
    /// <param name="layer">O Layer a ser verificado dentro do LayerMask.</param>
    /// <returns>True se o LayerMask contém o Layer; caso contrário, False.</returns>
    public static bool Contains(this LayerMask layerMask, int layer)
    {
        // Usa bit-shifting para verificar se a camada está no LayerMask
        return (layerMask.value & (1 << layer)) != 0;
    }

    public static async Task DoInLoopInUpdateTime(Action action, bool LoopCondition)
    {
        int delaytoNext = (int)Time.deltaTime;
#if !UNITY_WEBGL || UNITY_EDITOR

        await Task.Run(async () =>
        {
            //action.Invoke();
            while (LoopCondition)
            {
                action?.Invoke();

                //WaitAndDo(delaytoNext, action);
                //Debug.Log("DoInLoopInUpdateTime");

                delaytoNext = (int)Time.deltaTime;
                //await Task.Delay(1000);
                //await Task.Delay(1);
                await WaitForEndOfFrame();
            }
        });
        await Task.Yield();
#elif UNITY_WEBGL && !UNITY_EDITOR
        IE_DoInLoopInUpdateTime(action, LoopCondition).Run();
#endif
    }

    static IEnumerator IE_DoInLoopInUpdateTime(Action action, bool LoopCondition)
    {
        //action.Invoke();
        while (LoopCondition)
        {
            action?.Invoke();
            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// Plugin used to check appropriately if some object is realy null
    /// <br><i>The usual method (obj==null) on Unity still shows false when te object was destroyed (Missing)</i></br>
    /// <br>by: NSE</br>
    /// </summary>
    /// <returns></returns>
    public static bool IsNullOrDestroyed(this System.Object obj)
    {
        if (object.ReferenceEquals(obj, null)) return true;

        if (obj is UnityEngine.Object) return (obj as UnityEngine.Object) == null;

        return false;
    }



    public static void Run(this IEnumerator _coroutine, MonoBehaviour caster = null)
    {
        if (caster.IsNullOrDestroyed())
            CoroutineHolder.Instance.StartCoroutine(_coroutine);
        else
            caster.StartCoroutine(_coroutine);
    }

    public static void Stop(this IEnumerator _coroutine)
    {
        CoroutineHolder.Instance.StopCoroutine(_coroutine);
    }

    /// <summary>
    /// Uses Bitwise to compare value to a specific flag.
    /// <br>ex: in a 3x3 Grid Enum : A1|B2|C3 Contains(A1) retorna true </br>
    /// <br>Note the correct way to 'Sum' values, where the correct is A | B; A + B its incorrect.</br>
    /// <br>by: NSE</br>
    /// </summary>
    /// <param name="flag">Value to be Compared</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool Contains<T>(this T value, T flag) where T : Enum
    {
        if (value.GetType() != flag.GetType())
            throw new ArgumentException($"Enum Value must to be the same as the flag on Contains Plugin.<br> value is type of {value.GetType()}, flag is type of {flag.GetType()}");

        ulong num_value = Convert.ToUInt64(value);
        ulong num_flag = Convert.ToUInt64(flag);
        return (num_value & num_flag) == num_flag;
    }

    public static string ToBynaryString(this short value)
    {
        const int size = 16; // Um short tem 16 bits

        StringBuilder binaryString = new StringBuilder(size);
        for (int i = size - 1; i >= 0; i--)
        {
            int mask = 1 << i;
            binaryString.Append((value & mask) != 0 ? '1' : '0');
        }

        return binaryString.ToString();
    }

    [Serializable]
    public class DynamicList<T>
    {
        public List<T> list;
    }

    public static string MountPath(params string[] paths)
    {
        //string path = "";
        //for (int i = 0; i < paths.Length; i++)
        //{
        //    if(i>0)
        //        path = 
        //}
        //return path;
        if (paths.Contains(""))
        {
            string path = "";
            paths.ToList().ForEach(p => path += $"{p}\\");
            Debug.LogError("MountPath receives an emty or null value on ");
        }

        string result = Path.Combine(paths);
        result = result.Replace('\\', '/');
        return result;
    }



    /// <summary>
    /// retorna o DateTime Universal com base em um GTM definido (padrao -3, Brasil)
    /// </summary>
    /// <returns></returns>

    public static DateTime GetCurrentTimeData(int GTM = -3) => DateTime.UtcNow.AddHours(GTM);

    /// <summary>
    /// Retorna um DateTime em formato de string reconhecivel para SQL, util para interagir com HTTP Requests e interacao com Banco de dados do SQL
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static string SQLDateTimeFormat(this DateTime date) => date.ToString("yyyy-MM-dd HH:mm:ss");


    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
    public static float InverseLerp(Quaternion a, Quaternion b, Quaternion value)
    {
        Quaternion AB = new Quaternion(b.x - a.x, b.y - a.y, b.z - a.z, b.w - a.w);
        //Quaternion AV = value - a;
        Quaternion AV = new Quaternion(value.x - a.x, value.y - a.y, value.z - a.z, value.w - a.w);
        return Quaternion.Dot(AV, AB) / Quaternion.Dot(AB, AB);
    }
    /// <summary>
    /// Transforms Rotation from Local to World
    /// </summary>
    public static Quaternion TransformAngle(this Transform target, Quaternion rotation)
    {
        return target.rotation * Quaternion.LookRotation(rotation * Vector3.forward, rotation * Vector3.up);
    }
    /// <summary>
    /// Transforms Rotation From Global to Referenced Local
    /// </summary>
    public static Quaternion InverseTransformAngle(this Transform target, Quaternion rotation)
    {
        Vector3 forward = target.InverseTransformDirection(rotation * Vector3.forward);
        Vector3 up = target.InverseTransformDirection(rotation * Vector3.up);
        return Quaternion.LookRotation(forward, up);
        //return Quaternion.Inverse(target.rotation) * rotation;
    }
    public static bool IsFileLocked(string filePth)
    {
        try
        {
            using (FileStream stream = File.Open(filePth, FileMode.Open))
                stream.Close();
        }
        catch (IOException)
        {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            return true;
        }

        //file is not locked
        return false;
    }

    public static void ReloadDomain()
    {
#if UNITY_EDITOR
        EditorUtility.RequestScriptReload();
#endif
    }

    public static void ClearConsole()
    {
#if UNITY_EDITOR
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
#endif

    }
}



public static class DEVExtensions
{
    /// <summary>
    /// funcao que executa apenas no Editur para marcar o GameObject como alterado na cena;
    /// util para quando os valores em scripts sao alterados em editor via script, onde tais mudancas nao sao detectadas por padrao pelo Unity Editor.
    /// </summary>
    /// <param name="gameObject"></param>
    public static void SetDirty(this GameObject gameObject)
    {

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            MonoBehaviour[] scripts = gameObject.GetComponents<MonoBehaviour>();
            if (scripts.Length > 0)
                foreach (var script in scripts)
                    script.SetDirty();

            EditorUtility.SetDirty(gameObject);
        }
#endif
    }

    public static void SetDirty(this MonoBehaviour script)
    {

#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorUtility.SetDirty(script);
#endif
    }
    public static T[] Append<T>(this T[] array, T item)
    {
        if (array == null)
        {
            return new T[] { item };
        }
        T[] result = new T[array.Length + 1];
        array.CopyTo(result, 0);
        result[array.Length] = item;
        return result;
    }


    public static bool IsEmail(this string text)
    {
        try
        {
            var email = new MailAddress(text);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }

        //var trimmedEmail = text.Trim();

        ////return !string.IsNullOrEmpty(email.text) && email.text
        //if (trimmedEmail.EndsWith(".") || string.IsNullOrEmpty(text))
        //{
        //    return false; // suggested by @TK-421
        //}
        //try
        //{
        //    var addr = new System.Net.Mail.MailAddress(text);
        //    return addr.Address.CompareTo(trimmedEmail)==0;
        //}
        //catch
        //{
        //    return false;
        //}
    }

    public static bool PasswordIsSafePass(this string pass, int minimalChars = 8, bool needUpper = true, bool needNumber = true, bool needSecialChar = true)
    {
        bool passwordIsSafe = false;


        //bool PasswordMatch = !string.IsNullOrEmpty(pass) && !string.IsNullOrEmpty(confirmPass) && pass.CompareTo(confirmPass) == 0;

        if (string.IsNullOrEmpty(pass))
            return passwordIsSafe;


        bool hasUpperLetter = !needUpper || pass.Any(ch => char.IsLetter(ch) && char.IsUpper(ch));
        bool hasnumber = !needNumber || pass.Any(ch => char.IsNumber(ch));
        bool hasspecialChar = !needSecialChar || pass.Any(ch => !char.IsLetterOrDigit(ch));

        passwordIsSafe = hasnumber && hasspecialChar && hasUpperLetter && pass.Length >= minimalChars;

        return passwordIsSafe;
    }

    public static string PadCenter(this string input, int totalWidth, char paddingChar = ' ')
    {
        string l = input.PadLeft(totalWidth, paddingChar);
        string r = input.PadRight(totalWidth, paddingChar);

        string result = l + (r.Substring(input.Length));
        //result = input.PadRight(pr, paddingChar);
        return result;
    }

    public static void DebugColliderBounds(this Collider collider, Color color)
    {
        Bounds _bounds = collider.bounds;

        _bounds.DebugBounds(color);
    }

    public static void DebugBounds(this Bounds _bounds, Color color, float duration = -1)
    {
        if (duration < 0)
            duration = Time.deltaTime / 10;


        Vector3 bounds_v1_bot = new Vector3(_bounds.max.x, _bounds.min.y, _bounds.max.z);
        Vector3 bounds_v2_bot = new Vector3(_bounds.min.x, _bounds.min.y, _bounds.max.z);
        Vector3 bounds_v3_bot = _bounds.min;
        Vector3 bounds_v4_bot = new Vector3(_bounds.max.x, _bounds.min.y, _bounds.min.z);



        Debug.DrawLine(bounds_v1_bot, bounds_v2_bot, color, duration);
        Debug.DrawLine(bounds_v2_bot, bounds_v3_bot, color, duration);
        Debug.DrawLine(bounds_v3_bot, bounds_v4_bot, color, duration);
        Debug.DrawLine(bounds_v4_bot, bounds_v1_bot, color, duration);

        Vector3 bounds_v1_top = _bounds.max;
        Vector3 bounds_v2_top = new Vector3(_bounds.min.x, _bounds.max.y, _bounds.max.z);
        Vector3 bounds_v3_top = new Vector3(_bounds.min.x, _bounds.max.y, _bounds.min.z);
        Vector3 bounds_v4_top = new Vector3(_bounds.max.x, _bounds.max.y, _bounds.min.z);

        Debug.DrawLine(bounds_v1_top, bounds_v2_top, color, duration);
        Debug.DrawLine(bounds_v2_top, bounds_v3_top, color, duration);
        Debug.DrawLine(bounds_v3_top, bounds_v4_top, color, duration);
        Debug.DrawLine(bounds_v4_top, bounds_v1_top, color, duration);


        Debug.DrawLine(bounds_v1_bot, bounds_v1_top, color, duration);
        Debug.DrawLine(bounds_v2_bot, bounds_v2_top, color, duration);
        Debug.DrawLine(bounds_v3_bot, bounds_v3_top, color, duration);
        Debug.DrawLine(bounds_v4_bot, bounds_v4_top, color, duration);
    }

    public static bool BoolFromYesNoString(this string value)
    {
        value = value.ToUpper();
        if (value.CompareTo("YES") == 0)
            return true;
        else if (value.CompareTo("NO") == 0)
            return false;

        else
            throw new Exception($"Invalid string Value given on BoolFromYesNoString");
    }

    public static byte[] EncryptAes(this string text, string b64_key, string b64_iv)
    {
        using (Aes myAes = Aes.Create())
        {
            byte[] _key = Convert.FromBase64String(b64_key);
            byte[] _iv = Convert.FromBase64String(b64_iv);
            myAes.Key = _key;
            myAes.IV = _iv;
            // Criptografa o texto
            byte[] encrypted = EncryptStringToBytes_Aes(text, myAes.Key, myAes.IV);

            // Descriptografa o texto
            return encrypted;
        }
    }
    public static string DecryptStringFromBytes_Aes(this byte[] encryptedBytes, string b64_key, string b64_iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            byte[] _key = Convert.FromBase64String(b64_key);
            byte[] _iv = Convert.FromBase64String(b64_iv);
            aesAlg.Key = _key;
            aesAlg.IV = _iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
    static byte[] EncryptStringToBytes_Aes(string textToEncrypt, byte[] Key, byte[] IV)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(textToEncrypt);
                    }
                }
                return msEncrypt.ToArray();
            }
        }
    }

    public static string ToJson<A, B>(this Dictionary<A, B> dictionary)
    {
        try
        {
            string result = "{";
            foreach (var item in dictionary)
            {
                string key = item.Key.ToString();
                string value = item.Value.ToString();
                if (item.Key.GetType() == typeof(string))
                    key = $"\"{key}\"";
                if (item.Value.GetType() == typeof(string))
                    value = $"\"{value}\"";

                //result += "{";
                result += $"{key}:{value}";
                //result += "}";
                result += ",";

            }
            if (result[result.Length - 1] == ',')
                result = result.Substring(0, result.Length - 1);
            result += "}";
            return result;

        }
        catch
        {
            return "ERR";
        }
    }
    ///// <summary>
    ///// TransformPoint Equivalent to rotation
    ///// </summary>
    ///// <returns></returns>
    //public static Quaternion TransformAngle(this Transform transform,Quaternion rotation)
    //{
    //    return Quaternion.Inverse(transform.rotation) * rotation;
    //}
    ///// <summary>
    ///// InverseTransformPoint Equivalent to rotation
    ///// </summary>
    ///// <returns></returns>
    //public static Quaternion InverseTransformAngle(this Transform transform, Quaternion rotation)
    //{
    //    return transform.rotation * rotation;
    //}

    /// <summary>
    /// Transforms position from world space to local space
    /// </summary>
    public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
    {
        Matrix4x4 worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
        return worldToLocal.MultiplyPoint3x4(position);
    }

    /// <summary>
    /// Transforms position from local space to world space
    /// </summary>
    public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
    {
        Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        return localToWorld.MultiplyPoint3x4(position);
    }

    public static Transform FindChildRecursive(this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(name))
                return child;

            var result = child.FindChildRecursive(name);
            if (result != null)
                return result;
        }
        return null;
    }

    /// <summary>
    /// Simplifica strings em UTF8 para ser comativel com exibicao em consoles que nao aceitam caracrteres 
    /// especiais, resumindo apra caracteres compativeis com ASCII.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string SimplifyToASCII(this string input)
    {
        if (input == null) return null;

        // Normaliza o texto para decompor caracteres acentuados em caracteres base + acento
        string normalized = input.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        foreach (char c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);

        return sb.ToString().Normalize(NormalizationForm.FormC); // Re-normaliza para evitar problemas com caracteres compostos
    }

    public static string GeneratePassword(int length)
    {
        const string allowedChars = "abcdefghijklmnopqrstuvwxyz0123456789";
        //const string allowedSpecailChars = "!@#$%&-_+";
        //const string allowedCAPS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //const string allowedNums = "0123456789";

        using (var rng = new RNGCryptoServiceProvider())
        {
            var result = new char[length];
            var buffer = new byte[sizeof(uint)];
            for (int i = 0; i < length; i++)
            {

                rng.GetBytes(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                result[i] = allowedChars[(int)(num % (uint)allowedChars.Length)];
            }

            return new string(result);
        }
        //generatedPass = new Guid().GetHashCode();
    }
    public static Vector3 Multiply(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static void LerpPositionAndRotation(this Transform transform, Vector3 targetPosition, Quaternion targetRotation, float lerpFactor)
    {
        transform.SetPositionAndRotation(
            Vector3.Lerp(transform.position, targetPosition, lerpFactor),
            Quaternion.Slerp(transform.rotation, targetRotation, lerpFactor)
        );
    }


    public static void LerpLocalPositionAndRotation(this Transform transform, Vector3 targetPosition, Quaternion targetRotation, float lerpFactor)
    {
        transform.SetLocalPositionAndRotation(
            Vector3.Lerp(transform.localPosition, targetPosition, lerpFactor),
            Quaternion.Slerp(transform.localRotation, targetRotation, lerpFactor)
        );
        ;
    }
}
