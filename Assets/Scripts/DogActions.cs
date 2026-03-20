using UnityEngine;

public class DogActions : MonoBehaviour
{
    public Animator dogAnimator;

    public void Sit()
    {
        dogAnimator.SetTrigger("Sit");
    }

    public void Shake()
    {
        dogAnimator.SetTrigger("Shake");
    }

    public void Roll()
    {
        dogAnimator.SetTrigger("Roll");
    }

    public void Dead()
    {
        dogAnimator.SetTrigger("Dead");
    }
}