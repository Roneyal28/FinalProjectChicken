using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

namespace LazyBear.AnimalBehaviorClass
{
    public class AnimalBehaviour : MonoBehaviour
    {
        public float Speed;
        public float WalkDelay;
        public float WalkDuration;
        private bool Walking;
        private Vector2 RandomDir;
        private SpriteRenderer Sprite;
        private Animator Anim;
        public float WalkAreaSize;
        private Vector2 WalkAreaPos;
        public bool SpriteInRight;


        // Start is called once before the first execution of Update after the MonoBehaviour is created


        void Start()
        {
            WalkAreaPos = transform.position;
            StartCoroutine(Walk());
            Sprite = GetComponent<SpriteRenderer>();
            Anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Walking)
            {
                RandomDirMoviment();
                InvertDirection();
            }
        }

        void InvertDirection()
        {
            if (transform.position.x - 1f < WalkAreaPos.x - WalkAreaSize / 2)
            {
                RandomDir.x = Mathf.Abs(RandomDir.x);
                if (!SpriteInRight)
                {
                    Sprite.flipX = true;
                }
                if (SpriteInRight)
                {
                    Sprite.flipX = false;
                }
            }
            if (transform.position.x + 1f > WalkAreaPos.x + WalkAreaSize / 2)
            {
                RandomDir.x = -Mathf.Abs(RandomDir.x);
                if (!SpriteInRight)
                {
                    Sprite.flipX = false;
                }
                if (SpriteInRight)
                {
                    Sprite.flipX = true;
                }
            }
            if (transform.position.y + 1f > WalkAreaPos.y + WalkAreaSize / 2)
            {
                RandomDir.y = -Mathf.Abs(RandomDir.y);
            }
            if (transform.position.y - 1f < WalkAreaPos.y - WalkAreaSize / 2)
            {
                RandomDir.y = Mathf.Abs(RandomDir.y);

            }

        }

        void CreateRandomDirection()
        {
            RandomDir = Random.insideUnitCircle.normalized;
        }

        void RandomDirMoviment()
        {

            transform.Translate(RandomDir * Speed * Time.deltaTime);
        }
        private IEnumerator Walk()
        {
            yield return new WaitForSeconds(Random.Range(WalkDelay * 0.20f, WalkDelay * 1.80f));
            CreateRandomDirection();
            Walking = true;
            Anim.SetBool("Walk", true);
            if (RandomDir.x < 0)
            {
                if (!SpriteInRight)
                {
                    Sprite.flipX = false;
                }
                if (SpriteInRight)
                {
                    Sprite.flipX = true;
                }
            }
            else
            {
                if (!SpriteInRight)
                {
                    Sprite.flipX = true;
                }
                if (SpriteInRight)
                {
                    Sprite.flipX = false;
                }
            }
            yield return new WaitForSeconds(Random.Range(WalkDuration * 0.50f, WalkDuration * 1.50f));
            Walking = false;
            Anim.SetBool("Walk", false);
            int RandomRest = Random.Range(0, 10);
            if (RandomRest > 7)
            {
                Anim.SetBool("Rest", true);
            }
            else
            {
                Anim.SetBool("Rest", false);
            }
            StartCoroutine(Walk());
        }


        private void OnDrawGizmosSelected()
        { 
            Gizmos.DrawWireCube(transform.position, new Vector2(WalkAreaSize, WalkAreaSize));
            Gizmos.DrawLine(new Vector2(transform.position.x - 1, transform.position.y), new Vector2(transform.position.x + 1, transform.position.y));
            Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y - 1), new Vector2(transform.position.x, transform.position.y + 1));
        }
    }
}