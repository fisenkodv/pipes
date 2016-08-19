using System;
using System.Collections.Generic;

namespace Pipeline
{
  public class Pipeline
  {
    private readonly object _firstArg;

    private object _arg;

    private readonly List<IInvokable> _steps = new List<IInvokable>();

    private Pipeline( object firstArg )
    {
      _firstArg = firstArg;
      _arg = firstArg;
    }

    public static Step<TInput, TOutput> Start<TInput, TOutput>(
     Func<TInput> firstArg,
     Func<TInput, TOutput> firstStep )
    {
      return Pipe( firstArg, x => x.Invoke() )
        .Pipe( firstStep );
    }

    public static Step<TInput, TOutput> Pipe<TInput, TOutput>(
      TInput firstArg,
      Func<TInput, TOutput> firstStep )
    {
      var p = new Pipeline( firstArg );
      return new Step<TInput, TOutput>( p, firstStep );
    }

    public static Step Do( Action firstStep )
    {
      var p = new Pipeline( null );
      return new Step( p, firstStep );
    }

    public object Execute()
    {
      _arg = _firstArg;
      foreach ( IInvokable t in _steps )
      {
        _arg = t.Invoke();
      }

      return _arg;
    }

    public abstract class StepBase
    {
      protected Pipeline Pipeline;

      public Step Do( Action action )
      {
        if ( action == null ) throw new ArgumentNullException( nameof( action ) );
        return new Step( Pipeline, action );
      }
    }

    public class Step : StepBase, IInvokable
    {
      private readonly Action _action;

      public Step( Pipeline pipeline, Action action )
      {
        Pipeline = pipeline;
        _action = action;
        Pipeline._steps.Add( this );
      }

      object IInvokable.Invoke()
      {
        _action.Invoke();
        return Pipeline._arg;
      }

      public void Execute() => Pipeline.Execute();
    }

    public class Step<TInput> : StepBase, IInvokable
    {
      private readonly Pipeline _pipe;

      private readonly Action<TInput> _action;

      public Step( Pipeline pipe, Action<TInput> action )
      {
        _pipe = pipe;
        _action = action;
        _pipe._steps.Add( this );
      }

      object IInvokable.Invoke()
      {
        _action.Invoke( (TInput)_pipe._arg );
        return _pipe._arg;
      }

      public void Execute() => Pipeline.Execute();
    }

    public class Step<TInput, TOutput> : StepBase, IInvokable
    {
      private readonly Pipeline _pipe;

      private readonly Func<TInput, TOutput> _func;

      internal Step( Pipeline pipe, Func<TInput, TOutput> func )
      {
        _pipe = pipe;
        _func = func;
        _pipe._steps.Add( this );
      }

      object IInvokable.Invoke() => _func.Invoke( (TInput)_pipe._arg );

      public Step<TOutput, TNext> Pipe<TNext>( Func<TOutput, TNext> func )
      {
        if ( func == null ) throw new ArgumentNullException( nameof( func ) );
        return new Step<TOutput, TNext>( _pipe, func );
      }

      public Step<TOutput> Finish( Action<TOutput> action )
      {
        if ( action == null ) throw new ArgumentNullException( nameof( action ) );
        return new Step<TOutput>( Pipeline, action );
      }

      public TOutput Execute() => (TOutput)_pipe.Execute();
    }

    internal interface IInvokable
    {
      object Invoke();
    }
  }
}