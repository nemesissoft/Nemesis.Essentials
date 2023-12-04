#if NEMESIS_BINARY_PACKAGE
namespace Nemesis.Essentials.Design
#else
namespace $rootnamespace$.Design
#endif
{
    #region Actions
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action1Ref<T1>(ref T1 arg1);

#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action1Ref<T1, in T2>(ref T1 arg1, T2 arg2);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action2Ref<in T1, T2>(T1 arg1, ref T2 arg2);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action12Ref<T1, T2>(ref T1 arg1, ref T2 arg2);

#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action1Ref<T1, in T2, in T3>(ref T1 arg1, T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action2Ref<in T1, T2, in T3>(T1 arg1, ref T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action3Ref<in T1, in T2, T3>(T1 arg1, T2 arg2, ref T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action12Ref<T1, T2, in T3>(ref T1 arg1, ref T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action23Ref<in T1, T2, T3>(T1 arg1, ref T2 arg2, ref T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action13Ref<T1, in T2, T3>(ref T1 arg1, T2 arg2, ref T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action123Ref<T1, T2, T3>(ref T1 arg1, ref T2 arg2, ref T3 arg3);


#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action1Out<T1>(out T1 arg1);

#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action1Out<T1, in T2>(out T1 arg1, T2 arg2);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action2Out<in T1, T2>(T1 arg1, out T2 arg2);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action12Out<T1, T2>(out T1 arg1, out T2 arg2);

#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action1Out<T1, in T2, in T3>(out T1 arg1, T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action2Out<in T1, T2, in T3>(T1 arg1, out T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action3Out<in T1, in T2, T3>(T1 arg1, T2 arg2, out T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action12Out<T1, T2, in T3>(out T1 arg1, out T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action23Out<in T1, T2, T3>(T1 arg1, out T2 arg2, out T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action13Out<T1, in T2, T3>(out T1 arg1, T2 arg2, out T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate void Action123Out<T1, T2, T3>(out T1 arg1, out T2 arg2, out T3 arg3);
    #endregion

    #region Funcs
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func1Ref<T1, out TResult>(ref T1 arg1);

#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func1Ref<T1, in T2, out TResult>(ref T1 arg1, T2 arg2);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func2Ref<in T1, T2, out TResult>(T1 arg1, ref T2 arg2);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func12Ref<T1, T2, out TResult>(ref T1 arg1, ref T2 arg2);

#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func1Ref<T1, in T2, in T3, out TResult>(ref T1 arg1, T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func2Ref<in T1, T2, in T3, out TResult>(T1 arg1, ref T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func3Ref<in T1, in T2, T3, out TResult>(T1 arg1, T2 arg2, ref T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func12Ref<T1, T2, in T3, out TResult>(ref T1 arg1, ref T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func23Ref<in T1, T2, T3, out TResult>(T1 arg1, ref T2 arg2, ref T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func13Ref<T1, in T2, T3, out TResult>(ref T1 arg1, T2 arg2, ref T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func123Ref<T1, T2, T3, out TResult>(ref T1 arg1, ref T2 arg2, ref T3 arg3);

#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func4Ref<in T1, in T2, in T3, T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, ref T4 arg4);


#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func1Out<T1, out TResult>(out T1 arg1);

#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func1Out<T1, in T2, out TResult>(out T1 arg1, T2 arg2);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func2Out<in T1, T2, out TResult>(T1 arg1, out T2 arg2);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func12Out<T1, T2, out TResult>(out T1 arg1, out T2 arg2);

#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func1Out<T1, in T2, in T3, out TResult>(out T1 arg1, T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func2Out<in T1, T2, in T3, out TResult>(T1 arg1, out T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func3Out<in T1, in T2, T3, out TResult>(T1 arg1, T2 arg2, out T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func12Out<T1, T2, in T3, out TResult>(out T1 arg1, out T2 arg2, T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func23Out<in T1, T2, T3, out TResult>(T1 arg1, out T2 arg2, out T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func13Out<T1, in T2, T3, out TResult>(out T1 arg1, T2 arg2, out T3 arg3);
#if NEMESIS_BINARY_PACKAGE
    public
#else
	internal
#endif
        delegate TResult Func123Out<T1, T2, T3, out TResult>(out T1 arg1, out T2 arg2, out T3 arg3);
    #endregion
}