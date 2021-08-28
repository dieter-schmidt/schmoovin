using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace NeoFPS.Samples
{
	public class MultiInputSlider :
		MultiInputFocusableWidget,
		IUpdateSelectedHandler,
		IBeginDragHandler,
		IDragHandler,
		IEndDragHandler,
		IPointerClickHandler,
		ICanvasElement
	{
		[SerializeField] private RectTransform m_IncrementButton = null;
        [SerializeField] private RectTransform m_DecrementButton = null;
        [SerializeField] private RectTransform m_SliderRect = null;
        [SerializeField] private RectTransform m_SliderBarRect = null;
        [SerializeField] private RectTransform m_SliderFillRect = null;
        [SerializeField] private RectTransform m_ReadoutRect = null;
        [SerializeField] private Text m_Readout = null;
        [SerializeField] private int m_MinValue = 0;
		[SerializeField] private int m_MaxValue = 100;
		[SerializeField] private int m_Value = 50;
        [SerializeField] private ValueChangeEvent m_OnValueChanged = new ValueChangeEvent();

        #region IBeginDragHandler, IDragHandler, IEndDragHandler implementations

        private bool m_DraggingSlider = false;
        private bool m_DraggingReadout = false;

		public void OnBeginDrag (PointerEventData eventData)
		{
			if (!MayInteract (eventData))
				return;
			
			// Check for input field
			if (RectTransformUtility.RectangleContainsScreenPoint (m_ReadoutRect, eventData.position))
			{
				m_DraggingReadout = true;
				return;
			}
			else
				DeactivateInputField ();
				
			// Check for slider bar
			if (RectTransformUtility.RectangleContainsScreenPoint (m_SliderRect, eventData.position))
			{
				m_DraggingSlider = true;
			}
		}

		public void OnDrag (PointerEventData eventData)
		{
			if (!MayInteract (eventData))
				return;
			
			if (m_DraggingSlider)
			{
				Vector2 local;
				RectTransformUtility.ScreenPointToLocalPointInRectangle (m_SliderBarRect, eventData.position, eventData.pressEventCamera, out local);
				SetNormalisedValue (local.x / m_SliderBarRect.rect.width, false);

				eventData.Use();
				return;
			}

			if (m_DraggingReadout)
			{
				Vector2 local;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Readout.rectTransform, eventData.position, eventData.pressEventCamera, out local);
				caretSelectPositionInternal = GetCharacterIndexFromPosition(local) + m_DrawStart;
				MarkGeometryAsDirty();

				m_DragPositionOutOfBounds = !RectTransformUtility.RectangleContainsScreenPoint(m_Readout.rectTransform, eventData.position, eventData.pressEventCamera);
				if (m_DragPositionOutOfBounds && m_DragCoroutine == null)
					m_DragCoroutine = StartCoroutine(MouseDragOutsideRect(eventData));

				eventData.Use();
			}
		}

		public void OnEndDrag (PointerEventData eventData)
		{
			if (!MayInteract (eventData))
				return;
			
			if (m_DraggingSlider)
			{
				m_DraggingSlider = false;
				Vector2 local;
				RectTransformUtility.ScreenPointToLocalPointInRectangle (m_SliderBarRect, eventData.position, eventData.pressEventCamera, out local);
				SetNormalisedValue (local.x / m_SliderBarRect.rect.width, true);
				return;
			}

			if (m_DraggingReadout)
			{
				m_DraggingReadout = false;
			}
		}

		#endregion

		#region Input Field

		[SerializeField]
		private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

		[SerializeField]
		[Range(0f, 4f)]
		private float m_CaretBlinkRate = 0.85f;

        private const float k_HScrollSpeed = 0.05f;
        private const float k_VScrollSpeed = 0.10f;

        private int m_CaretPosition = 0;
		private int m_CaretSelectPosition = 0;
		private RectTransform caretRectTrans = null;
		private UIVertex[] m_CursorVerts = null;
		private TextGenerator m_InputTextCache = null;
        private CanvasRenderer m_CachedInputRenderer = null;
        private bool m_PreventFontCallback = false;
		private Mesh m_Mesh = null;
        private bool m_AllowInput = false;
		private bool m_DragPositionOutOfBounds = false;
        private bool m_CaretVisible = false;
		private Coroutine m_BlinkCoroutine = null;
		private float m_BlinkStartTime = 0.0f;
		private int m_DrawStart = 0;
		private int m_DrawEnd = 0;
		private Coroutine m_DragCoroutine = null;
		private string m_OriginalText = "";
		private bool m_WasCanceled = false;
		private bool m_HasDoneFocusTransition = false;
        private int m_MaxReadoutCharacters = 0;

		protected Mesh mesh
		{
			get
			{
				if (m_Mesh == null)
					m_Mesh = new Mesh();
				return m_Mesh;
			}
		}

		protected TextGenerator cachedInputTextGenerator
		{
			get
			{
				if (m_InputTextCache == null)
					m_InputTextCache = new TextGenerator();
				return m_InputTextCache;
			}
		}

		private string m_ReadoutText = string.Empty;
		public string readoutText
		{
			get
			{
				return m_ReadoutText;
			}
			set
			{
				if (this.readoutText == value)
					return;

				m_ReadoutText = value;

				#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					UpdateLabel();
					return;
				}
				#endif

				if (m_Readout != null)
				{
					if (m_CaretPosition > m_ReadoutText.Length)
						m_CaretPosition = m_CaretSelectPosition = m_ReadoutText.Length;
					UpdateLabel();
				}
			}
		}

		public bool readoutFocused
		{
			get { return m_AllowInput; }
		}

		public float caretBlinkRate
		{
			get { return m_CaretBlinkRate; }
			set
			{
				if (m_CaretBlinkRate != value)
				{
					m_CaretBlinkRate = value;
					if (m_AllowInput)
						SetCaretActive();
				}
			}
		}

		protected void ClampPos(ref int pos)
		{
			if (pos < 0) pos = 0;
			else if (pos > readoutText.Length) pos = readoutText.Length;
		}

		/// <summary>
		/// Current position of the cursor.
		/// Getters are public Setters are protected
		/// </summary>

		protected int caretPositionInternal { get { return m_CaretPosition + Input.compositionString.Length; } set { m_CaretPosition = value; ClampPos(ref m_CaretPosition); } }
		protected int caretSelectPositionInternal { get { return m_CaretSelectPosition + Input.compositionString.Length; } set { m_CaretSelectPosition = value; ClampPos(ref m_CaretSelectPosition); } }
		private bool hasSelection { get { return caretPositionInternal != caretSelectPositionInternal; } }

		/// <summary>
		/// Get: Returns the focus position as thats the position that moves around even during selection.
		/// Set: Set both the anchor and focus position such that a selection doesn't happen
		/// </summary>

		public int caretPosition
		{
			get { return m_CaretSelectPosition + Input.compositionString.Length; }
			set { selectionAnchorPosition = value; selectionFocusPosition = value; }
		}

		/// <summary>
		/// Get: Returns the fixed position of selection
		/// Set: If Input.compositionString is 0 set the fixed position
		/// </summary>

		public int selectionAnchorPosition
		{
			get { return m_CaretPosition + Input.compositionString.Length; }
			set
			{
				if (Input.compositionString.Length != 0)
					return;

				m_CaretPosition = value;
				ClampPos(ref m_CaretPosition);
			}
		}

		/// <summary>
		/// Get: Returns the variable position of selection
		/// Set: If Input.compositionString is 0 set the variable position
		/// </summary>

		public int selectionFocusPosition
		{
			get { return m_CaretSelectPosition + Input.compositionString.Length; }
			set
			{
				if (Input.compositionString.Length != 0)
					return;

				m_CaretSelectPosition = value;
				ClampPos(ref m_CaretSelectPosition);
			}
		}

//		private void OnValidateReadout()
//		{
//			//This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
//			if (!IsActive())
//				return;
//
//			m_Readout.text = m_Value.ToString ();
//			UpdateLabel();
//			if (m_AllowInput)
//				SetCaretActive();
//		}

		private void InitialiseReadout ()
		{
			if (m_Readout != null)
			{
				GameObject go = new GameObject(transform.name + " Input Caret");
				go.hideFlags = HideFlags.DontSave;
				go.transform.SetParent(m_Readout.transform.parent);
				go.transform.SetAsFirstSibling();
				go.layer = gameObject.layer;

				caretRectTrans = go.AddComponent<RectTransform>();
				m_CachedInputRenderer = go.AddComponent<CanvasRenderer>();
				m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

				// Needed as if any layout is present we want the caret to always be the same as the text area.
				go.AddComponent<LayoutElement>().ignoreLayout = true;

				AssignPositioningIfNeeded();

				m_MaxReadoutCharacters = GetMaxCharacters ();
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			m_DrawStart = 0;
			m_DrawEnd = m_ReadoutText.Length;
			if (m_Readout != null)
			{
				m_Readout.RegisterDirtyVerticesCallback(MarkGeometryAsDirty);
				m_Readout.RegisterDirtyVerticesCallback(UpdateLabel);
				UpdateLabel();
			}
			StartCoroutine (DelayedAlignBars ());
		}

		IEnumerator DelayedAlignBars ()
		{
			// Temporary hack because I the rect transforms don't seem to be properly set
			// up for awake, start, or onenable the first time round, annoyingly.
			// Need a better solution
			yield return null;
			m_SliderFillRect.sizeDelta = new Vector2 (
				m_SliderBarRect.rect.width * normalisedValue,
				0f
			);
            // Fix child rects randomly resizing
            Transform t = transform;
            if (t.childCount == 2)
            {
                RectTransform rt = (RectTransform)t.GetChild(1);
                rt.anchoredPosition = new Vector2(1f, 0f);
            }
        }

		protected override void OnDisable()
		{
			// the coroutine will be terminated, so this will ensure it restarts when we are next activated
			m_BlinkCoroutine = null;

			DeactivateInputField();
			if (m_Readout != null)
			{
				m_Readout.UnregisterDirtyVerticesCallback(MarkGeometryAsDirty);
				m_Readout.UnregisterDirtyVerticesCallback(UpdateLabel);
			}
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

			if (m_CachedInputRenderer)
				m_CachedInputRenderer.SetMesh(null);

			if (m_Mesh)
				DestroyImmediate(m_Mesh);
			m_Mesh = null;

			base.OnDisable();
		}

		IEnumerator CaretBlink()
		{
			// Always ensure caret is initially visible since it can otherwise be confusing for a moment.
			m_CaretVisible = true;
			yield return null;

			while (readoutFocused && m_CaretBlinkRate > 0)
			{
				// the blink rate is expressed as a frequency
				float blinkPeriod = 1f / m_CaretBlinkRate;

				// the caret should be ON if we are in the first half of the blink period
				bool blinkState = (Time.unscaledTime - m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2;
				if (m_CaretVisible != blinkState)
				{
					m_CaretVisible = blinkState;
					UpdateGeometry();
				}

				// Then wait again.
				yield return null;
			}
			m_BlinkCoroutine = null;
		}

		void SetCaretVisible()
		{
			if (!m_AllowInput)
				return;

			m_CaretVisible = true;
			m_BlinkStartTime = Time.unscaledTime;
			SetCaretActive();
		}

		// SetCaretActive will not set the caret immediately visible - it will wait for the next time to blink.
		// However, it will handle things correctly if the blink speed changed from zero to non-zero or non-zero to zero.
		void SetCaretActive()
		{
			if (!m_AllowInput)
				return;

			if (m_CaretBlinkRate > 0.0f)
			{
				if (m_BlinkCoroutine == null)
					m_BlinkCoroutine = StartCoroutine(CaretBlink());
			}
			else
			{
				m_CaretVisible = true;
			}
		}

		protected void OnFocusReadout ()
		{
			SelectAll();
		}

		protected void SelectAll()
		{
			caretPositionInternal = readoutText.Length;
			caretSelectPositionInternal = 0;
		}

		public void MoveTextEnd(bool shift)
		{
			int position = readoutText.Length;

			if (shift)
			{
				caretSelectPositionInternal = position;
			}
			else
			{
				caretPositionInternal = position;
				caretSelectPositionInternal = caretPositionInternal;
			}
			UpdateLabel();
		}

		public void MoveTextStart(bool shift)
		{
			int position = 0;

			if (shift)
			{
				caretSelectPositionInternal = position;
			}
			else
			{
				caretPositionInternal = position;
				caretSelectPositionInternal = caretPositionInternal;
			}

			UpdateLabel();
		}

		static string clipboard
		{
			get
			{
				return GUIUtility.systemCopyBuffer;
			}
			set
			{
				GUIUtility.systemCopyBuffer = value;
			}
		}

		public Vector2 ScreenToLocal(Vector2 screen)
		{
			var theCanvas = m_Readout.canvas;
			if (theCanvas == null)
				return screen;

			Vector3 pos = Vector3.zero;
			if (theCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				pos = m_Readout.transform.InverseTransformPoint(screen);
			}
			else if (theCanvas.worldCamera != null)
			{
				Ray mouseRay = theCanvas.worldCamera.ScreenPointToRay(screen);
				float dist;
				Plane plane = new Plane(m_Readout.transform.forward, m_Readout.transform.position);
				plane.Raycast(mouseRay, out dist);
				pos = m_Readout.transform.InverseTransformPoint(mouseRay.GetPoint(dist));
			}
			return new Vector2(pos.x, pos.y);
		}

		private int GetUnclampedCharacterLineFromPosition(Vector2 pos, TextGenerator generator)
		{
			return 0;
		}

		protected int GetCharacterIndexFromPosition(Vector2 pos)
		{
			TextGenerator gen = m_Readout.cachedTextGenerator;

			if (gen.lineCount == 0)
				return 0;

			int line = GetUnclampedCharacterLineFromPosition(pos, gen);
			if (line < 0)
				return 0;
			if (line >= gen.lineCount)
				return gen.characterCountVisible;

			int startCharIndex = gen.lines[line].startCharIdx;
			int endCharIndex = GetLineEndPosition(gen, line);

			for (int i = startCharIndex; i < endCharIndex; i++)
			{
				if (i >= gen.characterCountVisible)
					break;

				UICharInfo charInfo = gen.characters[i];
				Vector2 charPos = charInfo.cursorPos / m_Readout.pixelsPerUnit;

				float distToCharStart = pos.x - charPos.x;
				float distToCharEnd = charPos.x + (charInfo.charWidth / m_Readout.pixelsPerUnit) - pos.x;
				if (distToCharStart < distToCharEnd)
					return i;
			}

			return endCharIndex;
		}

		private bool MayInteract (PointerEventData eventData)
		{
			return IsActive () &&
				IsInteractable () &&
				eventData.button == PointerEventData.InputButton.Left &&
				m_Readout != null;
		}

		WaitForSeconds khYield = null;
        IEnumerator MouseDragOutsideRect(PointerEventData eventData)
		{
			while (m_DraggingReadout && m_DragPositionOutOfBounds)
			{
				Vector2 localMousePos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Readout.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePos);

				Rect rect = m_Readout.rectTransform.rect;

				if (localMousePos.x < rect.xMin)
					MoveLeft(true, false);
				else if (localMousePos.x > rect.xMax)
					MoveRight(true, false);

				UpdateLabel();

				if (khYield == null)
					khYield = new WaitForSeconds(k_HScrollSpeed);
				yield return khYield;
			}
			m_DragCoroutine = null;
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			if (!MayInteract(eventData))
				return;

			EventSystem.current.SetSelectedGameObject(gameObject, eventData);

			bool hadFocusBefore = m_AllowInput;
			base.OnPointerDown(eventData);

			// Only set caret position if we didn't just get focus now.
			// Otherwise it will overwrite the select all on focus.
			if (hadFocusBefore)
			{
				Vector2 pos = ScreenToLocal(eventData.position);
				caretSelectPositionInternal = caretPositionInternal = GetCharacterIndexFromPosition(pos) + m_DrawStart;
			}
			UpdateLabel();
			eventData.Use();
		}

		protected enum EditState
		{
			Continue,
			Finish
		}

		protected EditState KeyPressed(Event evt)
		{
			var currentEventModifiers = evt.modifiers;
			RuntimePlatform rp = Application.platform;
			bool isMac = (rp == RuntimePlatform.OSXEditor || rp == RuntimePlatform.OSXPlayer);
			bool ctrl = isMac ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
			bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
			bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
			bool ctrlOnly = ctrl && !alt && !shift;

			switch (evt.keyCode)
			{
				case KeyCode.Backspace:
					{
						Backspace();
						return EditState.Continue;
					}

				case KeyCode.Delete:
					{
						ForwardSpace();
						return EditState.Continue;
					}

				case KeyCode.Home:
					{
						MoveTextStart(shift);
						return EditState.Continue;
					}

				case KeyCode.End:
					{
						MoveTextEnd(shift);
						return EditState.Continue;
					}

					// Select All
				case KeyCode.A:
					{
						if (ctrlOnly)
						{
							SelectAll();
							return EditState.Continue;
						}
						break;
					}

					// Copy
				case KeyCode.C:
					{
						if (ctrlOnly)
						{
							clipboard = GetSelectedString();
							return EditState.Continue;
						}
						break;
					}

					// Paste
				case KeyCode.V:
					{
						if (ctrlOnly)
						{
							Append(clipboard);
							return EditState.Continue;
						}
						break;
					}

					// Cut
				case KeyCode.X:
					{
						if (ctrlOnly)
						{
							clipboard = GetSelectedString();
							Delete();
							UpdateLabel();
							return EditState.Continue;
						}
						break;
					}

				case KeyCode.LeftArrow:
					{
						MoveLeft(shift, ctrl);
						return EditState.Continue;
					}

				case KeyCode.RightArrow:
					{
						MoveRight(shift, ctrl);
						return EditState.Continue;
					}

					// Submit
				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					{
						return EditState.Finish;
					}

				case KeyCode.Escape:
					{
						m_WasCanceled = true;
						return EditState.Finish;
					}
			}

			char c = evt.character;
			// Dont allow return chars or tabulator key to be entered into single line fields.
			if (c == '\t' || c == '\r' || c == 10)
				return EditState.Continue;

			// Convert carriage return and end-of-text characters to newline.
			if (c == '\r' || (int)c == 3)
				c = '\n';

			if (IsValidChar(c))
			{
				Append(c);
			}

			if (c == 0)
			{
				if (Input.compositionString.Length > 0)
				{
					UpdateLabel();
				}
			}
			return EditState.Continue;
		}

		private bool IsValidChar(char c)
		{
			// Delete key on mac
			if ((int)c == 127)
				return false;
			// Accept newline and tab
			if (c == '\t' || c == '\n')
				return true;

			return m_Readout.font.HasCharacter(c);
		}

		private int GetMaxCharacters ()
		{
			int max = (m_MinValue < 0) ? 1 : 0;
			int greatest = Mathf.Max (m_MaxValue, Mathf.Abs (m_MinValue));
			for (int i = 1;; i *= 10)
			{
				if (greatest / i > 0)
					++max;
				else
					break;
			}
		
			return max;
		}

		/// <summary>
		/// Handle the specified event.
		/// </summary>
		private Event m_ProcessingEvent = new Event();

		public void ProcessEvent(Event e)
		{
			KeyPressed(e);
		}

		public virtual void OnUpdateSelected(BaseEventData eventData)
		{
			if (!readoutFocused)
				return;

			bool consumedEvent = false;
			while (Event.PopEvent(m_ProcessingEvent))
			{
				if (m_ProcessingEvent.rawType == EventType.KeyDown)
				{
					consumedEvent = true;
					var shouldContinue = KeyPressed(m_ProcessingEvent);
					if (shouldContinue == EditState.Finish)
					{
						DeactivateInputField();
						break;
					}
				}

				switch (m_ProcessingEvent.type)
				{
					case EventType.ValidateCommand:
					case EventType.ExecuteCommand:
						switch (m_ProcessingEvent.commandName)
						{
							case "SelectAll":
								SelectAll();
								consumedEvent = true;
								break;
						}
						break;
				}
			}

			if (consumedEvent)
				UpdateLabel();

			eventData.Use();
		}

		private string GetSelectedString()
		{
			if (!hasSelection)
				return "";

			int startPos = caretPositionInternal;
			int endPos = caretSelectPositionInternal;

			// Ensure pos is always less then selPos to make the code simpler
			if (startPos > endPos)
			{
				int temp = startPos;
				startPos = endPos;
				endPos = temp;
			}

			return readoutText.Substring(startPos, endPos - startPos);
		}

		private void MoveRight(bool shift, bool ctrl)
		{
			if (hasSelection && !shift)
			{
				// By convention, if we have a selection and move right without holding shift,
				// we just place the cursor at the end.
				caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
				return;
			}

			int position = caretSelectPositionInternal + 1;

			if (shift)
				caretSelectPositionInternal = position;
			else
				caretSelectPositionInternal = caretPositionInternal = position;
		}

		private void MoveLeft(bool shift, bool ctrl)
		{
			if (hasSelection && !shift)
			{
				// By convention, if we have a selection and move left without holding shift,
				// we just place the cursor at the start.
				caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
				return;
			}

			int position = caretSelectPositionInternal - 1;

			if (shift)
				caretSelectPositionInternal = position;
			else
				caretSelectPositionInternal = caretPositionInternal = position;
		}

		private int DetermineCharacterLine(int charPos, TextGenerator generator)
		{
			return 0;
		}

		/// <summary>
		///  Use cachedInputTextGenerator as the y component for the UICharInfo is not required
		/// </summary>

		private int LineUpCharacterPosition(int originalPos, bool goToFirstChar)
		{
			if (originalPos >= cachedInputTextGenerator.characterCountVisible)
				return 0;

			UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
			int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);

			// We are on the last line return last character
			if (originLine - 1 < 0)
				return goToFirstChar ? 0 : originalPos;


			int endCharIdx = cachedInputTextGenerator.lines[originLine].startCharIdx - 1;

			for (int i = cachedInputTextGenerator.lines[originLine - 1].startCharIdx; i < endCharIdx; ++i)
			{
				if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
					return i;
			}
			return endCharIdx;
		}

		/// <summary>
		///  Use cachedInputTextGenerator as the y component for the UICharInfo is not required
		/// </summary>

		private int LineDownCharacterPosition(int originalPos, bool goToLastChar)
		{
			if (originalPos >= cachedInputTextGenerator.characterCountVisible)
				return readoutText.Length;

			UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
			int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);

			// We are on the last line return last character
			if (originLine + 1 >= cachedInputTextGenerator.lineCount)
				return goToLastChar ? readoutText.Length : originalPos;

			// Need to determine end line for next line.
			int endCharIdx = GetLineEndPosition(cachedInputTextGenerator, originLine + 1);

			for (int i = cachedInputTextGenerator.lines[originLine + 1].startCharIdx; i < endCharIdx; ++i)
			{
				if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
					return i;
			}
			return endCharIdx;
		}

		private void Delete()
		{
			if (caretPositionInternal == caretSelectPositionInternal)
				return;

			if (caretPositionInternal < caretSelectPositionInternal)
			{
				m_ReadoutText = readoutText.Substring(0, caretPositionInternal) + readoutText.Substring(caretSelectPositionInternal, readoutText.Length - caretSelectPositionInternal);
				caretSelectPositionInternal = caretPositionInternal;
			}
			else
			{
				m_ReadoutText = readoutText.Substring(0, caretSelectPositionInternal) + readoutText.Substring(caretPositionInternal, readoutText.Length - caretPositionInternal);
				caretPositionInternal = caretSelectPositionInternal;
			}
		}

		private void ForwardSpace()
		{
			if (hasSelection)
			{
				Delete();
				UpdateLabel();
			}
			else
			{
				if (caretPositionInternal < readoutText.Length)
				{
					m_ReadoutText = readoutText.Remove(caretPositionInternal, 1);
					UpdateLabel();
				}
			}
		}

		private void Backspace()
		{
			if (hasSelection)
			{
				Delete();
				UpdateLabel();
			}
			else
			{
				if (caretPositionInternal > 0)
				{
					m_ReadoutText = readoutText.Remove(caretPositionInternal - 1, 1);
					caretSelectPositionInternal = caretPositionInternal = caretPositionInternal - 1;
					UpdateLabel();
				}
			}
		}

		// Insert the character and update the label.
		private void Insert(char c)
		{
			string replaceString = c.ToString();
			Delete();

			// Can't go past the character limit
			if (m_MaxReadoutCharacters > 0 && readoutText.Length >= m_MaxReadoutCharacters)
				return;

			m_ReadoutText = readoutText.Insert(m_CaretPosition, replaceString);
			caretSelectPositionInternal = caretPositionInternal += replaceString.Length;
		}

		protected void SendOnSubmit()
		{
			SetValue (int.Parse (m_ReadoutText), true);
		}

		/// <summary>
		/// Append the specified text to the end of the current.
		/// </summary>

		protected virtual void Append(string input)
		{
			for (int i = 0, imax = input.Length; i < imax; ++i)
			{
				char c = input[i];

				if (c >= ' ')
				{
					Append(c);
				}
			}
		}

		protected virtual void Append(char input)
		{
			// Validate the input
			input = Validate(readoutText, caretPositionInternal, input);

			// If the input is invalid, skip it
			if (input == 0)
				return;

			// Append the character and update the label
			Insert(input);
		}

		/// <summary>
		/// Update the visual text Text.
		/// </summary>

		protected void UpdateLabel()
		{
			if (m_Readout != null && m_Readout.font != null && !m_PreventFontCallback)
			{
				// TextGenerator.Populate invokes a callback that's called for anything
				// that needs to be updated when the data for that font has changed.
				// This makes all Text components that use that font update their vertices.
				// In turn, this makes the InputField that's associated with that Text component
				// update its label by calling this UpdateLabel method.
				// This is a recursive call we want to prevent, since it makes the InputField
				// update based on font data that didn't yet finish executing, or alternatively
				// hang on infinite recursion, depending on whether the cached value is cached
				// before or after the calculation.
				//
				// This callback also occurs when assigning text to our Text component, i.e.,
				// m_TextComponent.text = processed;

				m_PreventFontCallback = true;

				string fullText;
				if (Input.compositionString.Length > 0)
					fullText = readoutText.Substring(0, m_CaretPosition) + Input.compositionString + readoutText.Substring(m_CaretPosition);
				else
					fullText = readoutText;

				string processed = fullText;

				bool isEmpty = string.IsNullOrEmpty(fullText);

				// If not currently editing the text, set the visible range to the whole text.
				// The UpdateLabel method will then truncate it to the part that fits inside the Text area.
				// We can't do this when text is being edited since it would discard the current scroll,
				// which is defined by means of the m_DrawStart and m_DrawEnd indices.
				if (!m_AllowInput)
				{
					m_DrawStart = 0;
					m_DrawEnd = m_ReadoutText.Length;
				}

				if (!isEmpty)
				{
					// Determine what will actually fit into the given line
					Vector2 extents = m_Readout.rectTransform.rect.size;

					var settings = m_Readout.GetGenerationSettings(extents);
					settings.generateOutOfBounds = true;

					cachedInputTextGenerator.Populate(processed, settings);

					SetDrawRangeToContainCaretPosition(caretSelectPositionInternal);

					processed = processed.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, processed.Length) - m_DrawStart);

					SetCaretVisible();
				}
				m_Readout.text = processed;
				MarkGeometryAsDirty();
				m_PreventFontCallback = false;
			}
		}

		private bool IsSelectionVisible()
		{
			if (m_DrawStart > caretPositionInternal || m_DrawStart > caretSelectPositionInternal)
				return false;

			if (m_DrawEnd < caretPositionInternal || m_DrawEnd < caretSelectPositionInternal)
				return false;

			return true;
		}

		private static int GetLineStartPosition(TextGenerator gen, int line)
		{
			line = Mathf.Clamp(line, 0, gen.lines.Count - 1);
			return gen.lines[line].startCharIdx;
		}

		private static int GetLineEndPosition(TextGenerator gen, int line)
		{
			line = Mathf.Max(line, 0);
			if (line + 1 < gen.lines.Count)
				return gen.lines[line + 1].startCharIdx;
			return gen.characterCountVisible;
		}

		private void SetDrawRangeToContainCaretPosition(int caretPos)
		{
			// the extents gets modified by the pixel density, so we need to use the generated extents since that will be in the same 'space' as
			// the values returned by the TextGenerator.lines[x].height for instance.
			Vector2 extents = cachedInputTextGenerator.rectExtents.size;

			var characters = cachedInputTextGenerator.characters;
			if (m_DrawEnd > cachedInputTextGenerator.characterCountVisible)
				m_DrawEnd = cachedInputTextGenerator.characterCountVisible;

			float width = 0.0f;
			if (caretPos > m_DrawEnd || (caretPos == m_DrawEnd && m_DrawStart > 0))
			{
				// fit characters from the caretPos leftward
				m_DrawEnd = caretPos;
				for (m_DrawStart = m_DrawEnd - 1; m_DrawStart >= 0; --m_DrawStart)
				{
					if (width + characters[m_DrawStart].charWidth > extents.x)
						break;

					width += characters[m_DrawStart].charWidth;
				}
				++m_DrawStart;  // move right one to the last character we could fit on the left
			}
			else
			{
				if (caretPos < m_DrawStart)
					m_DrawStart = caretPos;

				m_DrawEnd = m_DrawStart;
			}

			// fit characters rightward
			for (; m_DrawEnd < cachedInputTextGenerator.characterCountVisible; ++m_DrawEnd)
			{
				width += characters[m_DrawEnd].charWidth;
				if (width > extents.x)
					break;
			}
		}

		private void MarkGeometryAsDirty()
		{
            #if UNITY_EDITOR
            if (!Application.isPlaying || UnityEditor.PrefabUtility.GetPrefabInstanceHandle(gameObject) != null)
				return;
            #endif

            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
		}

		public virtual void Rebuild(CanvasUpdate update)
		{
			switch (update)
			{
				case CanvasUpdate.LatePreRender:
					UpdateGeometry();
					break;
			}
		}

		public virtual void LayoutComplete()
		{}

		public virtual void GraphicUpdateComplete()
		{}

		private void UpdateGeometry()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif

			if (m_CachedInputRenderer == null)
				return;

			OnFillVBO(mesh);
			m_CachedInputRenderer.SetMesh(mesh);
		}

		private void AssignPositioningIfNeeded()
		{
			if (m_Readout != null && caretRectTrans != null &&
				(caretRectTrans.localPosition != m_Readout.rectTransform.localPosition ||
					caretRectTrans.localRotation != m_Readout.rectTransform.localRotation ||
					caretRectTrans.localScale != m_Readout.rectTransform.localScale ||
					caretRectTrans.anchorMin != m_Readout.rectTransform.anchorMin ||
					caretRectTrans.anchorMax != m_Readout.rectTransform.anchorMax ||
					caretRectTrans.anchoredPosition != m_Readout.rectTransform.anchoredPosition ||
					caretRectTrans.sizeDelta != m_Readout.rectTransform.sizeDelta ||
					caretRectTrans.pivot != m_Readout.rectTransform.pivot))
			{
				caretRectTrans.localPosition = m_Readout.rectTransform.localPosition;
				caretRectTrans.localRotation = m_Readout.rectTransform.localRotation;
				caretRectTrans.localScale = m_Readout.rectTransform.localScale;
				caretRectTrans.anchorMin = m_Readout.rectTransform.anchorMin;
				caretRectTrans.anchorMax = m_Readout.rectTransform.anchorMax;
				caretRectTrans.anchoredPosition = m_Readout.rectTransform.anchoredPosition;
				caretRectTrans.sizeDelta = m_Readout.rectTransform.sizeDelta;
				caretRectTrans.pivot = m_Readout.rectTransform.pivot;
			}
		}

		private void OnFillVBO(Mesh vbo)
		{
			using (var helper = new VertexHelper())
			{
				if (!readoutFocused)
				{
					helper.FillMesh(vbo);
					return;
				}

				Rect inputRect = m_Readout.rectTransform.rect;
				Vector2 extents = inputRect.size;

				// get the text alignment anchor point for the text in local space
				Vector2 textAnchorPivot = Text.GetTextAnchorPivot(m_Readout.alignment);
				Vector2 refPoint = Vector2.zero;
				refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
				refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);

				// Ajust the anchor point in screen space
				Vector2 roundedRefPoint = m_Readout.PixelAdjustPoint(refPoint);

				// Determine fraction of pixel to offset text mesh.
				// This is the rounding in screen space, plus the fraction of a pixel the text anchor pivot is from the corner of the text mesh.
				Vector2 roundingOffset = roundedRefPoint - refPoint + Vector2.Scale(extents, textAnchorPivot);
				roundingOffset.x = roundingOffset.x - Mathf.Floor(0.5f + roundingOffset.x);
				roundingOffset.y = roundingOffset.y - Mathf.Floor(0.5f + roundingOffset.y);

				if (!hasSelection)
					GenerateCursor(helper, roundingOffset);
				else
					GenerateHightlight(helper, roundingOffset);

				helper.FillMesh(vbo);
			}
		}

		private void GenerateCursor(VertexHelper vbo, Vector2 roundingOffset)
		{
			if (!m_CaretVisible)
				return;

			if (m_CursorVerts == null)
				CreateCursorVerts();

			float width = 3f;
			float height = m_Readout.fontSize;
			int adjustedPos = Mathf.Max(0, caretPositionInternal - m_DrawStart);
			TextGenerator gen = m_Readout.cachedTextGenerator;

			if (gen == null)
				return;

			if (m_Readout.resizeTextForBestFit)
				height = gen.fontSizeUsedForBestFit / m_Readout.pixelsPerUnit;

			Vector2 startPosition = Vector2.zero;

			// Calculate startPosition
			if (gen.characterCountVisible + 1 > adjustedPos || adjustedPos == 0)
			{
				UICharInfo cursorChar = gen.characters[adjustedPos];
				startPosition.x = cursorChar.cursorPos.x;
				startPosition.y = cursorChar.cursorPos.y;
			}
			startPosition.x /= m_Readout.pixelsPerUnit;

			// Should Only clamp when Text uses horizontal word wrap.
			if (startPosition.x > m_Readout.rectTransform.rect.xMax)
				startPosition.x = m_Readout.rectTransform.rect.xMax;

			startPosition.y = m_Readout.rectTransform.rect.center.y + height / 2;

			Color c = m_Readout.color;
			float halfWidth = width * 0.5f;
			m_CursorVerts [0].position = new Vector3(startPosition.x - halfWidth, startPosition.y - height, 0.0f);
			m_CursorVerts [0].color = c;
			m_CursorVerts [1].position = new Vector3(startPosition.x + halfWidth, startPosition.y - height, 0.0f);
			m_CursorVerts [1].color = c;
			m_CursorVerts [2].position = new Vector3(startPosition.x + halfWidth, startPosition.y, 0.0f);
			m_CursorVerts [2].color = c;
			m_CursorVerts [3].position = new Vector3(startPosition.x - halfWidth, startPosition.y, 0.0f);
			m_CursorVerts [3].color = c;

			if (roundingOffset != Vector2.zero)
			{
				for (int i = 0; i < m_CursorVerts.Length; i++)
				{
					UIVertex uiv = m_CursorVerts[i];
					uiv.position.x += roundingOffset.x;
					uiv.position.y += roundingOffset.y;
				}
			}

			vbo.AddUIVertexQuad(m_CursorVerts);

			startPosition.y = Screen.height - startPosition.y;

			Input.compositionCursorPos = startPosition;
		}

		private void CreateCursorVerts()
		{
			m_CursorVerts = new UIVertex[4];

			for (int i = 0; i < m_CursorVerts.Length; i++)
			{
				m_CursorVerts[i] = UIVertex.simpleVert;
				m_CursorVerts [i].color = m_SelectionColor;// m_TextComponent.color;
				m_CursorVerts[i].uv0 = Vector2.zero;
			}
		}

		private float SumLineHeights(int endLine, TextGenerator generator)
		{
			float height = 0.0f;
			for (int i = 0; i < endLine; ++i)
			{
				height += generator.lines[i].height;
			}

			return height;
		}

		private void GenerateHightlight(VertexHelper vbo, Vector2 roundingOffset)
		{
			if (m_CursorVerts == null)
				CreateCursorVerts();

			int startChar = Mathf.Max(0, caretPositionInternal - m_DrawStart);
			int endChar = Mathf.Max(0, caretSelectPositionInternal - m_DrawStart);

			// Ensure pos is always less then selPos to make the code simpler
			if (startChar > endChar)
			{
				int temp = startChar;
				startChar = endChar;
				endChar = temp;
			}

			endChar -= 1;
			TextGenerator gen = m_Readout.cachedTextGenerator;

			float height = m_Readout.fontSize;

			if (m_Readout.resizeTextForBestFit)
				height = gen.fontSizeUsedForBestFit / m_Readout.pixelsPerUnit;

			Vector2 startPosition = Vector2.zero;
			Vector2 endPosition = Vector2.zero;
            
			UIVertex vert = UIVertex.simpleVert;
			vert.uv0 = Vector2.zero;
			vert.color = m_SelectionColor;

			startPosition.y = m_Readout.rectTransform.rect.center.y + height / 2;
			endPosition.y = startPosition.y - height;

			UICharInfo charInfo = gen.characters[startChar];
			startPosition.x = charInfo.cursorPos.x / m_Readout.pixelsPerUnit;
			charInfo = gen.characters[endChar];
			endPosition.x = (charInfo.cursorPos.x + charInfo.charWidth) / m_Readout.pixelsPerUnit;

			m_CursorVerts [0].position = new Vector3(startPosition.x, startPosition.y - height, 0.0f);
			m_CursorVerts [0].color = m_SelectionColor;
			m_CursorVerts [1].position = new Vector3(endPosition.x, startPosition.y - height, 0.0f);
			m_CursorVerts [1].color = m_SelectionColor;
			m_CursorVerts [2].position = new Vector3(endPosition.x, startPosition.y, 0.0f);
			m_CursorVerts [2].color = m_SelectionColor;
			m_CursorVerts [3].position = new Vector3(startPosition.x, startPosition.y, 0.0f);
			m_CursorVerts [3].color = m_SelectionColor;

			if (roundingOffset != Vector2.zero)
			{
				for (int i = 0; i < m_CursorVerts.Length; i++)
				{
					UIVertex uiv = m_CursorVerts[i];
					uiv.position.x += roundingOffset.x;
					uiv.position.y += roundingOffset.y;
				}
			}

			vbo.AddUIVertexQuad(m_CursorVerts);
		}

		/// <summary>
		/// Validate the specified input.
		/// </summary>

		private char Validate(string text, int pos, char ch)
		{
			// Integer and decimal
			bool cursorBeforeDash = (pos == 0 && text.Length > 0 && text[0] == '-');
			if (!cursorBeforeDash)
			{
				if (ch >= '0' && ch <= '9') return ch;
				if (ch == '-' && pos == 0) return ch;
				//                if (ch == '.' && characterValidation == CharacterValidation.Decimal && !text.Contains(".")) return ch;
			}

			return (char)0;
		}

		public void ActivateInputField()
		{
			if (m_Readout == null || m_Readout.font == null || !IsActive() || !IsInteractable())
				return;

			if (!readoutFocused)
				StartCoroutine (DelayedActivateInputField ());
		}

		WaitForEndOfFrame eofYield;
		private IEnumerator DelayedActivateInputField()
		{
			if (eofYield == null)
				eofYield = new WaitForEndOfFrame ();
			yield return eofYield;

			if (EventSystem.current.currentSelectedGameObject != gameObject)
				EventSystem.current.SetSelectedGameObject(gameObject);

			Input.imeCompositionMode = IMECompositionMode.On;
			OnFocusReadout();

			m_AllowInput = true;
			m_OriginalText = readoutText;
			m_WasCanceled = false;
			SetCaretVisible();
			UpdateLabel();
		}

		public void DeactivateInputField()
		{
			// Not activated do nothing.
			if (!m_AllowInput)
				return;

			m_HasDoneFocusTransition = false;
			m_AllowInput = false;

			if (m_Readout != null && IsInteractable())
			{
				if (m_WasCanceled)
					readoutText = m_OriginalText;

				m_CaretPosition = m_CaretSelectPosition = 0;

				SendOnSubmit();

				Input.imeCompositionMode = IMECompositionMode.Auto;
			}

			MarkGeometryAsDirty();
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			DeactivateInputField();
			base.OnDeselect(eventData);
		}

		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			if (m_HasDoneFocusTransition)
				state = SelectionState.Highlighted;
			else if (state == SelectionState.Pressed)
				m_HasDoneFocusTransition = true;

			base.DoStateTransition(state, instant);
		}

#endregion

#region IPointerClickHandler implementation

		public void OnPointerClick (PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			
			Vector2 pressPosition = eventData.pressPosition;

			// Check for increment / decrement buttons
			if (RectTransformUtility.RectangleContainsScreenPoint (m_IncrementButton, pressPosition))
			{
				DeactivateInputField();
				Increment ();
				return;
			}
			if (RectTransformUtility.RectangleContainsScreenPoint (m_DecrementButton, pressPosition))
			{
				DeactivateInputField();
				Decrement ();
				return;
			}

			// Check for slider bar
			if (!m_DraggingSlider && RectTransformUtility.RectangleContainsScreenPoint (m_SliderRect, pressPosition))
			{
				DeactivateInputField();
				Vector2 local;
				RectTransformUtility.ScreenPointToLocalPointInRectangle (m_SliderBarRect, pressPosition, eventData.pressEventCamera, out local);
				SetNormalisedValue (local.x / m_SliderBarRect.rect.width, true);
				return;
			}

			// Check for input field
			if (!m_DraggingReadout && RectTransformUtility.RectangleContainsScreenPoint (m_ReadoutRect, pressPosition))
			{
				ActivateInputField();
			}
		}

#endregion

		[Serializable]
		public class ValueChangeEvent : UnityEvent<int> {}

		public ValueChangeEvent onValueChanged
		{
			get { return m_OnValueChanged; }
		}

		public int value
		{
			get { return m_Value; }
			set { SetValue (value, true); }
		}

		public void SetLimits(int min, int max)
        {
			m_MinValue = min;
			m_MaxValue = max;

			SetValue(m_Value, true);

			//UpdateLabel();
		}

		public float normalisedValue
		{
			get { return (float)(m_Value - m_MinValue) / (float)(m_MaxValue - m_MinValue); }
			set { SetNormalisedValue (value, true); }
		}

		private void SetValue (int v, bool triggerEvent)
        {
            m_Value = Mathf.Clamp (v, m_MinValue, m_MaxValue);
			if (m_SliderBarRect != null && m_SliderFillRect != null)
			{
				float zeroedValue = (float)(m_Value - m_MinValue);
				float zeroedTotal = (float)(m_MaxValue - m_MinValue);
				m_SliderFillRect.sizeDelta = new Vector2 (
					m_SliderBarRect.rect.width * zeroedValue / zeroedTotal,
					0f
				);
			}
			readoutText = m_Value.ToString ();
			if (triggerEvent)
				m_OnValueChanged.Invoke (m_Value);
		}

		private void SetNormalisedValue (float n, bool triggerEvent)
		{
			float clamped = Mathf.Clamp01 (n);
			m_Value = (int)((float)(m_MaxValue - m_MinValue) * clamped) + m_MinValue;
			if (m_SliderBarRect != null && m_SliderFillRect != null)
			{
				m_SliderFillRect.sizeDelta = new Vector2 (
					m_SliderBarRect.rect.width * clamped,
					0f
				);
			}
			readoutText = m_Value.ToString ();
			if (triggerEvent)
				m_OnValueChanged.Invoke (m_Value);
		}

#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			m_MinValue = Mathf.Clamp (m_MinValue, 0, m_MaxValue);
			m_MaxValue = Mathf.Clamp (m_MaxValue, m_MinValue, 999);
			m_Value = Mathf.Clamp (m_Value, m_MinValue, m_MaxValue);

			//SetValue (m_Value, false);
		}
#endif

		protected override void Awake ()
		{
			base.Awake ();
			InitialiseReadout ();
			if (m_Value == -1)
				SetValue (m_Value, false);
		}

		public override void OnSubmit (BaseEventData eventData)
		{
			if (widgetState == WidgetState.Focussed)
			{
				widgetState = WidgetState.Highlighted;
				PlayAudio (MenuAudio.ClickValid);
			}
			else
				base.OnSubmit (eventData);
		}

		public override void FocusLeft ()
		{
			Decrement ();
		}

		public override void FocusRight ()
		{
			Increment ();
		}

		public void Increment ()
		{
			if (value < m_MaxValue)
			{
				++value;
				// Highlight right button
			}
			PlayAudio (MenuAudio.Move);
		}

		public void Decrement ()
		{
			if (value > 0)
			{
				--value;
				// Highlight left button
			}
			PlayAudio (MenuAudio.Move);
		}
	}
}
