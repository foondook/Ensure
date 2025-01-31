// Generated code - do not modify

export abstract class TodoListFeatureStepsBase {
    /**
     * When I navigate to \"/todos\"
     */
    abstract whenINavigateTo(param1: string): Promise<void>;

    /**
     * And I enter \"Buy groceries\" into \"new-todo\"
     */
    abstract andIEnterInto(param1: string, param2: string): Promise<void>;

    /**
     * And I press Enter
     */
    abstract andIPressEnter(): Promise<void>;

    /**
     * Then I should see text \"Buy groceries\" in the todo list
     */
    abstract thenIShouldSeeTextInTheTodoList(param1: string): Promise<void>;

    /**
     * Given I have a todo \"Buy groceries\"
     */
    abstract givenIHaveATodo(param1: string): Promise<void>;

    /**
     * And I click the checkbox next to \"Buy groceries\"
     */
    abstract andIClickTheCheckboxNextTo(param1: string): Promise<void>;

    /**
     * Then the todo \"Buy groceries\" should be marked as completed
     */
    abstract thenTheTodoShouldBeMarkedAsCompleted(param1: string): Promise<void>;

}
