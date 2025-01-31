// Generated code - do not modify

export abstract class LoginFeatureStepsBase {
    /**
     * Navigate to \"/login\"
     */
    abstract navigateTo(param1: string): Promise<void>;

    /**
     * Enter \"test@example.com\" into \"email\" field
     */
    abstract enterIntoField(param1: string, param2: string): Promise<void>;

    /**
     * Click \"Sign In\" button
     */
    abstract clickButton(param1: string): Promise<void>;

    /**
     * Verify text \"Welcome back\" is shown
     */
    abstract verifyTextIsShown(param1: string): Promise<void>;

}
