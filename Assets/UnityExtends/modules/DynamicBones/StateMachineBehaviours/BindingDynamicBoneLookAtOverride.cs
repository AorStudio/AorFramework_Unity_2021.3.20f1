using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.Universal.DynamicBones.StateMachineBehaviours
{

    public class BindingDynamicBoneLookAtOverride : StateMachineBehaviour
    {

        private DynamicBoneLookAtPramsOverride m_bindingItem;

        private void InitDynamicBoneLookAtPramsOverride(Animator animator, ref AnimatorStateInfo stateInfo)
        {
            int stateHash = stateInfo.shortNameHash;
            DynamicBoneLookAtPramsOverride[] items = animator.GetComponentsInChildren<DynamicBoneLookAtPramsOverride>(true);
            foreach (DynamicBoneLookAtPramsOverride item in items)
            {
                if (Animator.StringToHash(item.BindingAnimStateLabel) == stateHash)
                {
                    m_bindingItem = item;
                    break;
                }
            }
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            if (!m_bindingItem)
                InitDynamicBoneLookAtPramsOverride(animator, ref stateInfo);

            if (m_bindingItem)
                m_bindingItem.enabled = true;
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_bindingItem)
                m_bindingItem.enabled = false;
        }

    }

}


