namespace echo17.EndlessBook.Demo01
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using echo17.EndlessBook;

    /// <summary>
    /// Simple demo showing the easiest setup of the book
    /// </summary>
    public class Demo01 : MonoBehaviour
    {
        protected EndlessBook book;

        public float stateAnimationTime = 1f;
        public EndlessBook.PageTurnTimeTypeEnum turnTimeType = EndlessBook.PageTurnTimeTypeEnum.TotalTurnTime;
        public float turnTime = 1f;

        void Awake()
        {
            // cache the book
            book = GameObject.Find("Book").GetComponent<EndlessBook>();
        }

        void Update()
        {
            bool changeState = false;
            EndlessBook.StateEnum newState = EndlessBook.StateEnum.ClosedFront;

            // change the state of the book
            if (InputCompatibility.GetKeyDown(KeyCode.Z)) { changeState = true; newState = EndlessBook.StateEnum.ClosedFront; }
            else if (InputCompatibility.GetKeyDown(KeyCode.X)) { changeState = true; newState = EndlessBook.StateEnum.OpenFront; }
            else if (InputCompatibility.GetKeyDown(KeyCode.C)) { changeState = true; newState = EndlessBook.StateEnum.OpenMiddle; }
            else if (InputCompatibility.GetKeyDown(KeyCode.V)) { changeState = true; newState = EndlessBook.StateEnum.OpenBack; }
            else if (InputCompatibility.GetKeyDown(KeyCode.B)) { changeState = true; newState = EndlessBook.StateEnum.ClosedBack; }

            if (changeState)
            {
                book.SetState(newState, animationTime: stateAnimationTime, onCompleted: OnBookStateChanged);
            }

            // turn the page of the book

            bool turnToPage = false;
            int newPageNumber = 0;

            if (InputCompatibility.GetKeyDown(KeyCode.Alpha1)) { turnToPage = true; newPageNumber = 1; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Alpha2)) { turnToPage = true; newPageNumber = 2; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Alpha3)) { turnToPage = true; newPageNumber = 3; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Alpha4)) { turnToPage = true; newPageNumber = 4; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Alpha5)) { turnToPage = true; newPageNumber = 5; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Alpha6)) { turnToPage = true; newPageNumber = 6; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Alpha7)) { turnToPage = true; newPageNumber = 7; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Alpha8)) { turnToPage = true; newPageNumber = 8; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Alpha9)) { turnToPage = true; newPageNumber = 9; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Alpha0)) { turnToPage = true; newPageNumber = 10; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Keypad1)) { turnToPage = true; newPageNumber = 11; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Keypad2)) { turnToPage = true; newPageNumber = 12; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Keypad3)) { turnToPage = true; newPageNumber = 13; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Keypad4)) { turnToPage = true; newPageNumber = 14; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Keypad5)) { turnToPage = true; newPageNumber = 15; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Keypad6)) { turnToPage = true; newPageNumber = 16; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Keypad7)) { turnToPage = true; newPageNumber = 17; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Keypad8)) { turnToPage = true; newPageNumber = 18; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Keypad9)) { turnToPage = true; newPageNumber = 19; }
            else if (InputCompatibility.GetKeyDown(KeyCode.Keypad0)) { turnToPage = true; newPageNumber = 20; }

            if (turnToPage)
            {
                book.TurnToPage(newPageNumber, turnTimeType, turnTime,
                    openTime: stateAnimationTime,
                    onCompleted: OnBookTurnToPageCompleted,
                    onPageTurnStart: OnPageTurnStart,
                    onPageTurnEnd: OnPageTurnEnd
                    );
            }
        }

        public virtual void OnStateButtonClicked(int buttonIndex)
        {
            book.SetState((EndlessBook.StateEnum)buttonIndex, animationTime: stateAnimationTime, onCompleted: OnBookStateChanged);
        }

        public virtual void OnPageButtonClicked(int pageNumber)
        {
            book.TurnToPage(pageNumber == 999 ? book.LastPageNumber : pageNumber,
                turnTimeType, 
                turnTime,
                openTime: stateAnimationTime,
                onCompleted: OnBookTurnToPageCompleted,
                onPageTurnStart: OnPageTurnStart,
                onPageTurnEnd: OnPageTurnEnd
                );
        }

        public virtual void OnTurnButtonClicked(int direction)
        {
            if (direction == -1)
            {
                book.TurnBackward(turnTime,
                    onCompleted: OnBookTurnToPageCompleted,
                    onPageTurnStart: OnPageTurnStart,
                    onPageTurnEnd: OnPageTurnEnd);
            }
            else
            {
                book.TurnForward(turnTime,
                    onCompleted: OnBookTurnToPageCompleted,
                    onPageTurnStart: OnPageTurnStart,
                    onPageTurnEnd: OnPageTurnEnd);
            }
        }

        protected virtual void OnBookStateChanged(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int currentPageNumber)
        {
            Debug.Log("State set to " + toState + ". Current Page Number = " + currentPageNumber);
        }

        protected virtual void OnBookTurnToPageCompleted(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int currentPageNumber)
        {
            Debug.Log("OnBookTurnToPageCompleted: State set to " + toState + ". Current Page Number = " + currentPageNumber);
        }

        protected virtual void OnPageTurnStart(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            Debug.Log("OnPageTurnStart: front [" + pageNumberFront + "] back [" + pageNumberBack + "] fv [" + pageNumberFirstVisible + "] lv [" + pageNumberLastVisible + "] dir [" + turnDirection + "]");
        }

        protected virtual void OnPageTurnEnd(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            Debug.Log("OnPageTurnEnd: front [" + pageNumberFront + "] back [" + pageNumberBack + "] fv [" + pageNumberFirstVisible + "] lv [" + pageNumberLastVisible + "] dir [" + turnDirection + "]");
        }
    }
}