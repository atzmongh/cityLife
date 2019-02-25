using cityLife4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace cityLife4
{
    /// <summary>
    /// this class translates messages into the desired language
    /// </summary>
    public class TranslateBox
    {
        private string _targetLanguageCode;
        private string _defaultLanguageCode;
        private string _noTranslation;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="targetLanguageCode">the language code for which we need to translate each translation key (such as "en" or "ru")</param>
        /// <param name="defaultLanguageCode">the language to which we need to translate if the target translation language does not exist</param>
        /// <param name="noTranslation">what to do it there is no translation in the desired language. There are 2 options:
        /// "showAsterisks" - if there is no translation in the desired language - return the translation key, with leading and trailing asterisk, such as
        /// *monday*
        /// "defaultLanguage" - if there is no translation in the desired language:
        /// - look for translation in the default language, and if exists - reutnr it
        /// - otherwise - return the translation key without asterisks.
        /// The first option is for QA, the second option is for production
        /// </param>
        public TranslateBox(string targetLanguageCode, string defaultLanguageCode, string noTranslation )
        {
            _targetLanguageCode = targetLanguageCode;
            _defaultLanguageCode = defaultLanguageCode;
            _noTranslation = noTranslation;
        }

        public string targetLanguage
        {
            set
            {
                cityLifeDBContainer1 db = new cityLifeDBContainer1();
                if (db.Languages.Find(value) == null)
                {
                    //such language code does not exist in the language table
                    throw new AppException(106, null, "an unsupported language requested:" + value);
                }
                _targetLanguageCode = value;
        }
            get { return _targetLanguageCode; }
        }
        /// <summary>
        /// perform a translation operation to the target languagewhich was defined in the constructor. If the translation key does not exist
        /// creates a record in the translation key table, and also a record in the translation table for the English text. So default English
        /// translation will be created immediately. This translation can later be changed.
        /// </summary>
        /// <param name="translationKey">the translation key which needs to be translated</param>
        /// <param name="lineNumber">the line number in the code where the translate method was last called. Note that 
        /// if the same key is requested in more than one place in the code - the DB will only save the last call</param>
        /// <param name="filePath">the file path where the method was last called</param>
        /// <returns>the translation in the required language. If translation was not found in the required language - 
        /// will try to return the translation in the default language. Otherwise will return the trnaslation key
        /// itself. If "show asterisks" is set - then will reutnr the translation key with surounded with asterisks</returns>
        public string translate(string translationKey, 
                               [CallerLineNumber] int lineNumber = 0,
                               [CallerFilePath]   string filePath = null)
        {
            if (translationKey == null)
            {
                AppException.writeException(120, null, Environment.StackTrace);
                return "";
            }
            cityLifeDBContainer1 db = new cityLifeDBContainer1();
            TranslationKey theTranslationKey =  db.TranslationKeys.SingleOrDefault(record => record.transKey == translationKey);
            if (theTranslationKey==null)
            {
                //the translation key does not exist. Add it to the translation key table and add the
                //same text to the translation text as English text
                theTranslationKey = new TranslationKey() { transKey = translationKey, isUsed = true, filePath = filePath, lineNumber = lineNumber };
                db.TranslationKeys.Add(theTranslationKey);
                db.SaveChanges();
                Language english = db.Languages.Single(a => a.languageCode == "EN");
                Translation theEnglishTranslation = new Translation() { Language = english, TranslationKey = theTranslationKey, message = translationKey };
                db.Translations.Add(theEnglishTranslation);
                db.SaveChanges();
                if (_noTranslation == "showAsterisks")
                {
                    //return the translation key surrounded by asterisks for QA
                    string translated = '*' + translationKey + '*';
                    return translated;
                }
                else
                {
                    //assuming "defaultLanguage" - but since there is no translation at all for this key - return the key without asterisks
                    return translationKey;
                }
            }
            else
            {
                //The translation key exists -
                if (theTranslationKey.lineNumber != lineNumber || theTranslationKey.filePath != filePath)
                {
                    //The file path or the line number where the call was performed has changed. Assuming it is because we added some lines of
                    //code, or refactored the code - update the file path tne line number
                    theTranslationKey.lineNumber = lineNumber;
                    theTranslationKey.filePath = filePath;
                    db.SaveChanges();
                }
                //check if we have translation in the desired language
                Translation theTranslation = db.Translations.SingleOrDefault(record => record.TranslationKey.id == theTranslationKey.id && 
                                                                                     record.Language.languageCode == _targetLanguageCode);
                if (theTranslation != null)
                {
                    //the translation exists in the target language - return it
                    return theTranslation.message;
                }
                else
                {
                    //The translation does not exist in the target language
                    if (_noTranslation == "showAsterisks")
                    {
                        //In this case we need to return the translation key with asterisks
                        return '*' + translationKey + '*';
                    }
                    else
                    {
                        //assuming _noTranslation is "defaultLanguage" - look for translation in the default language
                        theTranslation = db.Translations.SingleOrDefault(record => record.TranslationKey.id == theTranslationKey.id &&
                                                                                 record.Language.languageCode == _defaultLanguageCode);
                        if (theTranslation != null)
                        {
                            //translation exists - return it
                            return theTranslation.message;
                        }
                        else
                        {
                            //no translation exists even in the default language - return the translation key (without asterisks)
                            return translationKey;
                        }
                    }
                }
            }
        }
    }
}