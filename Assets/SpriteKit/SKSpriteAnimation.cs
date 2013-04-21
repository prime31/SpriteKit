using UnityEngine;
using System.Collections;


public class SKSpriteAnimation
{
	private SKSpriteAnimationState _animationState;
	public SKSpriteAnimationState animationState
	{
		get { return _animationState; }
		set
		{
			_animationState = value;
			
			if( _animationState != null )
				initializeStateValues();
		}
	}
	
	public float delay;
	public int iterations;
	private float _secondsPerFrame;
	
	private SKSprite _sprite;
	
	// animation state
	private float _elapsedDelay; // total time delayed
	private bool _delayComplete; // once we complete the delay this gets set so we can reverse and play properly for the future
	
	private float _elapsedTime; // elapsed time for the current loop iteration
	private float _totalElapsedTime; // total elapsed time of the entire animation
	private float _duration; // duration for a single run
	private float _totalDuration;
	
	private bool _isPaused;
	private bool _isStopped;
	private bool _isReversed; // have we been reversed? this is different than a PingPong loop's backwards section
	private bool _isLoopingBackOnPingPong;
	private int _completedIterations;
	private int _currentFrame = -1;
	
	
	public SKSpriteAnimation( SKSprite sprite )
	{
		_sprite = sprite;
	}

	
	private void initializeStateValues()
	{
		stop();
		
		_secondsPerFrame = 1f / _animationState.framesPerSecond;
		delay = _animationState.delay;
		iterations = _animationState.iterations;
		_duration = _secondsPerFrame * _animationState.imageNames.Length;
		
		if( iterations < 0 )
			_totalDuration = float.PositiveInfinity;
		else
			_totalDuration = _duration * iterations;
		
		if( _animationState.playAutomatically )
			play();
	}

	
	/// <summary>
	/// tick method. if it returns true it indicates the animation is complete
	/// </summary>
	public bool tick( float deltaTime )
	{
		if( _isStopped )
			return true;
		
		if( _isPaused )
			return false;
		
		// handle delay and return if we are still delaying
		if( !_delayComplete && _elapsedDelay < delay )
		{
			_elapsedDelay += deltaTime;
			
			// are we done delaying?
			if( _elapsedDelay >= delay )
				_delayComplete = true;

			return false;
		}
		
		var isComplete = false;
		
		// handle applying speed
		deltaTime *= animationState.speed;
		
		// increment or decrement the total elapsed time then clamp from 0 to totalDuration
        if( _isReversed )
			_totalElapsedTime -= deltaTime;
        else
			_totalElapsedTime += deltaTime;
		
		_totalElapsedTime = Mathf.Clamp( _totalElapsedTime, 0, _totalDuration );
		
		// using our fresh totalElapsedTime, figure out what iteration we are on
		_completedIterations = (int)Mathf.Floor( _totalElapsedTime / _duration );
		
		// we can only be loopiong back on a PingPong if our loopType is PingPong and we are on an odd numbered iteration
		_isLoopingBackOnPingPong = false;
		if( animationState.wrapMode == WrapMode.PingPong )
		{
			// infinite loops and we are on an odd numbered iteration
			if( iterations < 0 && _completedIterations % 2 != 0 )
			{
				_isLoopingBackOnPingPong = true;
			}
			else if( iterations > 0 )
			{
				// we have finished all iterations and we went one over to a non looping back iteration
				// so we still count as looping back so that we finish in the proper location
				if( _completedIterations >= iterations && _completedIterations % 2 == 0 )
					_isLoopingBackOnPingPong = true;
				else if( _completedIterations < iterations && _completedIterations % 2 != 0 )
					_isLoopingBackOnPingPong = true;
			}
		}
		
		
		// figure out the current elapsedTime
		if( iterations > 0 && _completedIterations >= iterations )
		{
			// we finished all iterations so clamp to the end of this tick
			_elapsedTime = _duration;
			
			// if we arent reversed, we are done
			if( !_isReversed )
				isComplete = true;
		}
		else if( _totalElapsedTime < _duration )
		{
			_elapsedTime = _totalElapsedTime; // havent finished a single iteration yet
		}
		else
		{
			// TODO: when we increment a completed iteration (go from 0 to 1 for example) we should probably run through once setting
			// _elapsedTime = duration so that complete handlers in a chain or flow fire when expected
			_elapsedTime = _totalElapsedTime % _duration; // have finished at least one iteration
		}
		
		
		// check for completion when going in reverse
		if( _isReversed && _totalElapsedTime <= 0 )
			isComplete = true;

		
		// return true only if we are complete
		if( isComplete )
		{
			// fire off the completion handler and return true
			return true;
		}

		
		// if we are looping back on a PingPong loop
		var convertedElapsedTime = _isLoopingBackOnPingPong ? _duration - _elapsedTime : _elapsedTime;
		
		
		var desiredFrame = Mathf.FloorToInt( convertedElapsedTime / _secondsPerFrame );
		if( desiredFrame != _currentFrame )
		{
			_currentFrame = desiredFrame;
			_sprite.setUVs( animationState.textureInfo[_currentFrame].uvRect );
		}
		
		return false;
	}
	
	
	public void play()
	{
		_isPaused = _isStopped = false;
	}
	
	
	public void stop()
	{
		// reset all state
		// we reset CurrentFrame to zero to force play the first frame immediately
		_currentFrame = -1;
		_elapsedTime = _elapsedDelay = _totalElapsedTime = _completedIterations = 0;
		_isReversed = _isLoopingBackOnPingPong = false;
		_isStopped = true;
	}
	
	
	public void pause()
	{
		_isPaused = true;
	}

	
	public void restart()
	{
		stop();
		play();
	}
	
	
	public void reverse()
	{
		_isReversed = !_isReversed;
	}
	
	
	public void playForward()
	{
		_isReversed = false;
		play();
	}
	
	
	public void playReverse()
	{
		_isReversed = true;
	}

}
