import { legacy_createStore as createStore } from 'redux'

export interface RootState {
  sidebarShow: boolean
  sidebarUnfoldable: boolean
  theme: string
}

const initialState: RootState = {
  sidebarShow: true,
  sidebarUnfoldable: false,
  theme: 'light',
}

interface SetAction {
  type: 'set'
  sidebarShow?: boolean
  sidebarUnfoldable?: boolean
  theme?: string
}

type AppAction = SetAction | { type: string }

const changeState = (state = initialState, action: AppAction): RootState => {
  switch (action.type) {
    case 'set':
      const { type, ...rest } = action as SetAction
      return { ...state, ...rest }
    default:
      return state
  }
}

const store = createStore(changeState)
export default store
