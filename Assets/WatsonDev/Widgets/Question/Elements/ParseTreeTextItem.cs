﻿/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace IBM.Watson.Widgets.Question
{
	/// <summary>
	/// Controls ParseTreeTextItem view. Attached to ParseTreeTextItem prefab.
	/// </summary>
    public class ParseTreeTextItem : MonoBehaviour
    {
        [SerializeField]
        private Text m_ParseTreeTextField;

		[SerializeField]
		private RectTransform m_BoundingBox;

		private int m_AnimationTextColor = -1;
		private int m_AnimationTextScale = -1;
		private int m_AnimationTextAlpha = -1;

        private bool m_IsHighlighted = true;
        public bool IsHighlighted
        {
            get { return m_IsHighlighted; }
            set
            {
				if(m_IsHighlighted != value){
		            m_IsHighlighted = value;
		            m_RectTransform = m_ParseTreeTextField.gameObject.GetComponent<RectTransform>();
					StopAnimations();
					m_AnimationTextColor = LeanTween.textColor(m_RectTransform, m_IsHighlighted ? m_ColorLight : m_ColorDark, m_TransitionTime).id;
					m_AnimationTextScale = LeanTween.scale(m_RectTransform, m_IsHighlighted ? m_ScaleUpSize : m_ScaleDownSize, m_TransitionTime).id;
					m_AnimationTextAlpha = LeanTween.alpha(m_BoundingBox, IsHighlighted ? 1.0f : 0.0f, m_TransitionTime).id;
				}
            }
        }

        private string m_ParseTreeWord;
        public string ParseTreeWord
        {
            get { return m_ParseTreeWord; }
            set
            {
                m_ParseTreeWord = value;
                UpdateParseTreeTextField();
            }
        }

		/// <summary>
		/// Word position in the sentence.
		/// </summary>
		private long m_Position;
		public long Position
		{
			get { return m_Position; }
			set
			{
				m_Position = value;
			}
		}

		/// <summary>
		/// Part of speech of the word
		/// </summary>
        [SerializeField]
        private string m_POS;
        public string POS
        {
            get { return m_POS; }
            set
            {
                m_POS = value;
            }
        }

		/// <summary>
		/// Slot of the word
		/// </summary>
        [SerializeField]
        private string m_Slot;
        public string Slot
        {
            get { return m_Slot; }
            set { m_Slot = value; }
        }

		[SerializeField]
		private int m_ParentNode;
		public int ParentNode
		{
			get { return m_ParentNode; }
			set { m_ParentNode = value; }
		}

		public RectTransform m_ParentWordRectTransform { get; set; }

        public List<string> m_Features = new List<string>();
        private RectTransform m_RectTransform;
        private Color m_ColorLight = new Color(0.8f, 0.8f, 0.8f);
        private Color m_ColorDark = new Color(0.8f, 0.8f, 0.8f);
        private Vector3 m_ScaleUpSize = new Vector3(1.25f, 1.25f, 1.25f);
        private Vector3 m_ScaleDownSize = new Vector3(1f, 1f, 1f);
        private float m_TransitionTime = 0.5f;

        /// <summary>
        /// Sets a reference of the RectTransform.
        /// </summary>
        void Awake()
        {
            m_RectTransform = m_ParseTreeTextField.gameObject.GetComponent<RectTransform>();
        }

		void OnDisable(){
			StopAnimations ();
		}

        /// <summary>
        /// Update the ParseTree text view.
        /// </summary>
        private void UpdateParseTreeTextField()
        {
            m_ParseTreeTextField.text = ParseTreeWord;
			Invoke("UpdateBoundingBox", 1f);
        }

		/// <summary>
		/// Updates the bounding box.
		/// </summary>
		private void UpdateBoundingBox()
		{
			RectTransform textRectTransform = m_ParseTreeTextField.gameObject.GetComponent<RectTransform>();
			m_BoundingBox.pivot = new Vector2(0.5f, 0.5f);
			float boxWidth = textRectTransform.rect.width + 40f;
			float boxHeight = textRectTransform.rect.height + 30f;
			float boxX = textRectTransform.rect.x + textRectTransform.rect.width;// + 20f;
			float boxY = textRectTransform.rect.y;// - 15f;

			m_BoundingBox.sizeDelta = new Vector2(boxWidth, boxHeight);
			m_BoundingBox.anchoredPosition = new Vector2(boxX, boxY);
		}

		private void StopAnimations(){
			if (LeanTween.descr (m_AnimationTextColor) != null) {
				LeanTween.cancel(m_AnimationTextColor);
				m_AnimationTextColor = -1;
			}

			if (LeanTween.descr (m_AnimationTextScale) != null) {
				LeanTween.cancel(m_AnimationTextScale);
				m_AnimationTextScale = -1;
			}

			if (LeanTween.descr (m_AnimationTextAlpha) != null) {
				LeanTween.cancel(m_AnimationTextAlpha);
				m_AnimationTextAlpha = -1;
			}
		}
    }
}